
namespace AskLlm.CommandLine
{
    public class ConsoleSpinner : IDisposable
    {
        private static (int Left, int Top) _originalCursorPosition;
        private readonly CancellationTokenSource _cts;
        private readonly Task _spinnerTask;
        private bool _disposed = false;

        public ConsoleSpinner(string message = "Working")
        {
            _cts = new CancellationTokenSource();
            _spinnerTask = SpinAsync(message, _cts.Token);
        }

        private static async Task SpinAsync(string message, CancellationToken token)
        {
            var spinnerChars = new[] { '⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏' };
            var originalCursorVisible = Console.CursorVisible;
            Console.CursorVisible = false;

            var index = 0;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    Console.Write($"\r{spinnerChars[index]} {message}");
                    index = (index + 1) % spinnerChars.Length;
                    await Task.Delay(100, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            finally
            {
                // \r - Moves cursor to the beginning of the current line
                // Creates a string of spaces to overwrite the spinner text
                // Second \r - Moves cursor back to the beginning of the line again
                Console.Write($"\r{new string(' ', message.Length + 5)}\r");

                Console.CursorVisible = originalCursorVisible;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cts.Cancel();
                _spinnerTask.Wait(1000); // Wait up to 1 second for cleanup

                _cts.Dispose();
                _disposed = true;
            }
        }
    }
}
