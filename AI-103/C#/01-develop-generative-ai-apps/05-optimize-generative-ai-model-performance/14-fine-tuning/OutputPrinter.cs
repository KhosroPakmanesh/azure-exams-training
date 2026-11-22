using AzureExamsTraining.AI103.Shared;

internal static class OutputPrinter
{
    internal static void PrintIntroduction()
    {
        ConsoleChat.Banner("14: Fine-tuning data, job lifecycle, and evaluation");
        Console.WriteLine(
            "Commands:\n  prepare [output-directory]\n  validate <train.jsonl> [more.jsonl ...]\n" + "  upload <validated-file.jsonl>\n  create-job <train-file-id> <validation-file-id> <base-model> --confirm-costs\n" + "  status <fine-tuning-job-id>\n  compare <deployed-fine-tuned-model> <test.jsonl> [results.csv]\n");
    }

    internal static void PrintPrepared(string folderString)
    {
        Console.WriteLine(
            $"Generated 40 training, 12 validation, and 12 held-out test examples in {Path.GetFullPath(folderString)}.");
        Console.WriteLine(
            "Run 'validate' before uploading. These synthetic examples teach classification behavior, not policy facts.");
    }
}
