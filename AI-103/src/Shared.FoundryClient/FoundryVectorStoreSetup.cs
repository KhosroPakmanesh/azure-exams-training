using System.Text.Json;

namespace FoundryLabs.Shared;

/// <summary>
/// For the file-search lessons, accept either an existing vector store ID,
/// a directory of documents, or one or more document paths. Uploaded files are
/// attached to a new vector store, then polled until indexing has completed.
/// </summary>
public static class FoundryVectorStoreSetup
{
    public static async Task<string> ResolveAsync(FoundryToolRestClient client,
        string[] arguments, string storeName, bool pdfOnly = false,
        CancellationToken cancellationToken = default)
    {
        if (arguments.Length == 1 && arguments[0].StartsWith("vs_", StringComparison.Ordinal))
            return arguments[0];
        if (arguments.Length == 0)
            throw new ArgumentException(
                "Supply a document directory, one or more document paths, or an existing vs_... vector store ID.");

        var extensions = pdfOnly
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf" }
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".pdf", ".txt", ".md", ".docx" };
        var paths = new List<string>();
        foreach (var argument in arguments)
        {
            if (Directory.Exists(argument))
            {
                paths.AddRange(Directory.EnumerateFiles(argument, "*", SearchOption.TopDirectoryOnly)
                    .Where(p => extensions.Contains(Path.GetExtension(p))));
            }
            else if (File.Exists(argument) && extensions.Contains(Path.GetExtension(argument)))
            {
                paths.Add(argument);
            }
            else
            {
                throw new ArgumentException($"File/directory not found or unsupported type: {argument}");
            }
        }
        paths = paths.Select(Path.GetFullPath).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (paths.Count == 0)
            throw new ArgumentException("No supported documents found. Add files before starting this project.");

        using var created = await client.PostJsonAsync("vector_stores",
            new { name = storeName }, cancellationToken);
        var vectorStoreId = FoundryToolRestClient.RequireId(created);
        Console.WriteLine($"Created vector store: {vectorStoreId}");
        Console.WriteLine("Keep this ID to reuse the store in later runs without reuploading files.");
        Console.WriteLine("The store and uploaded files remain in your Azure resource until deleted.\n");

        foreach (var path in paths)
        {
            Console.Write($"Uploading/indexing {Path.GetFileName(path)} ... ");
            using var uploaded = await client.UploadFileAsync(path, cancellationToken);
            var fileId = FoundryToolRestClient.RequireId(uploaded);
            using var attached = await client.PostJsonAsync(
                $"vector_stores/{Uri.EscapeDataString(vectorStoreId)}/files",
                new { file_id = fileId }, cancellationToken);
            await WaitForIndexingAsync(client, vectorStoreId, fileId, cancellationToken);
            Console.WriteLine("ready");
        }
        Console.WriteLine();
        return vectorStoreId;
    }

    private static async Task WaitForIndexingAsync(FoundryToolRestClient client,
        string vectorStoreId, string fileId, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromMinutes(5));
        while (true)
        {
            using JsonDocument status = await client.GetAsync(
                $"vector_stores/{Uri.EscapeDataString(vectorStoreId)}/files/{Uri.EscapeDataString(fileId)}",
                timeout.Token);
            var value = status.RootElement.GetProperty("status").GetString();
            if (value == "completed") return;
            if (value is "failed" or "cancelled")
                throw new InvalidOperationException($"Indexing did not complete for {fileId}: {value}");
            if (value is not ("in_progress" or "pending"))
                throw new InvalidOperationException($"Unexpected indexing state: {value}");
            await Task.Delay(TimeSpan.FromSeconds(2), timeout.Token);
        }
    }
}
