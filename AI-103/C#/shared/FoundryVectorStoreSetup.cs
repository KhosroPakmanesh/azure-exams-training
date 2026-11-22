using System.Text.Json;

namespace AzureExamsTraining.AI103.Shared;
/// <summary>
/// For the file-search lessons, accept either an existing vector store ID,
/// a directory of documents, or one or more document paths. Uploaded files are
/// attached to a new vector store, then polled until indexing has completed.
/// </summary>
public static class FoundryVectorStoreSetup
{
    public static async Task<string> ResolveAsync(
        FoundryToolRestClient client,
        string[] arguments,
        string storeName,
        bool pdfOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (arguments.Length == 1 && arguments[0].StartsWith(
            "vs_",
            StringComparison.Ordinal))
        {
            return arguments[0];
        }

        if (arguments.Length == 0)
        {
            throw new ArgumentException(
                "Supply a document directory, one or more document paths, or an existing vs_... vector store ID.");
        }

        HashSet<string>? extensionStrings = pdfOnly ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf"
        }

        : new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf",
            ".txt",
            ".md",
            ".docx"
        };
        var pathStrings = new List<string>();
        foreach (string? argumentString in arguments)
        {
            if (Directory.Exists(argumentString))
            {
                pathStrings.AddRange(
                    Directory.EnumerateFiles(
                        argumentString,
                        "*",
                        SearchOption.TopDirectoryOnly).Where(pString => extensionStrings.Contains(Path.GetExtension(pString))));
            }
            else if (File.Exists(argumentString) && extensionStrings.Contains(Path.GetExtension(argumentString)))
            {
                pathStrings.Add(argumentString);
            }
            else
            {
                throw new ArgumentException($"File/directory not found or unsupported type: {argumentString}");
            }
        }

        pathStrings = pathStrings.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (pathStrings.Count == 0)
        {
            throw new ArgumentException("No supported documents found. Add files before starting this project.");
        }

        using JsonDocument? createdJsonDocument = await client.PostJsonAsync(
            "vector_stores",
            new
            {
                name = storeName
            },
            cancellationToken);
        string? vectorStoreIdString = FoundryToolRestClient.RequireId(createdJsonDocument);
        Console.WriteLine($"Created vector store: {vectorStoreIdString}");
        Console.WriteLine("Keep this ID to reuse the store in later runs without reuploading files.");
        Console.WriteLine("The store and uploaded files remain in your Azure resource until deleted.\n");
        foreach (string? pathString in pathStrings)
        {
            Console.Write($"Uploading/indexing {Path.GetFileName(pathString)} ... ");
            using JsonDocument? uploadedJsonDocument = await client.UploadFileAsync(
                pathString,
                cancellationToken);
            string? fileIdString = FoundryToolRestClient.RequireId(uploadedJsonDocument);
            using JsonDocument? attachedJsonDocument = await client.PostJsonAsync(
                $"vector_stores/{Uri.EscapeDataString(vectorStoreIdString)}/files",
                new
                {
                    file_id = fileIdString
                },
                cancellationToken);
            await WaitForIndexingAsync(
                client,
                vectorStoreIdString,
                fileIdString,
                cancellationToken);
            Console.WriteLine("ready");
        }

        Console.WriteLine();
        return vectorStoreIdString;
    }

    private static async Task WaitForIndexingAsync(
        FoundryToolRestClient foundryToolRestClient,
        string vectorStoreIdString,
        string fileIdString,
        CancellationToken cancellationToken)
    {
        using CancellationTokenSource? timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(5));
        while (true)
        {
            using JsonDocument statusJsonDocument = await foundryToolRestClient.GetAsync(
                $"vector_stores/{Uri.EscapeDataString(vectorStoreIdString)}/files/{Uri.EscapeDataString(fileIdString)}",
                timeoutCancellationTokenSource.Token);
            string? valueString = statusJsonDocument.RootElement.GetProperty("status").GetString();
            if (valueString == "completed")
            {
                return;
            }

            if (valueString is "failed" or "cancelled")
            {
                throw new InvalidOperationException($"Indexing did not complete for {fileIdString}: {valueString}");
            }

            if (valueString is not ("in_progress" or "pending"))
            {
                throw new InvalidOperationException($"Unexpected indexing state: {valueString}");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(2),
                timeoutCancellationTokenSource.Token);
        }
    }
}
