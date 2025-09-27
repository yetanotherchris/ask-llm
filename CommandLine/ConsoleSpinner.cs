
using System;
using System.Text;

namespace AskLlm.CommandLine
{
    public class ConsoleSpinner : IDisposable
    {
        private readonly CancellationTokenSource _cts;
        private readonly Task _spinnerTask;
        private bool _disposed = false;
        private readonly Encoding? _originalOutputEncoding;

        public ConsoleSpinner(string message = "Working")
        {
            // Set UTF-8 encoding for better Unicode support in PowerShell
            _originalOutputEncoding = Console.OutputEncoding;
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch
            {
                // If setting encoding fails, continue with original encoding
            }

            _cts = new CancellationTokenSource();
            _spinnerTask = SpinAsync(message, _cts.Token);
        }

        private static async Task SpinAsync(string message, CancellationToken token)
        {
            // Keep the original Braille pattern characters
            var spinnerChars = new[] { '⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏' };

            bool? originalCursorVisible = null;
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    originalCursorVisible = Console.CursorVisible;
                    Console.CursorVisible = false;
                }
                catch
                {
                    // If cursor visibility can't be changed, continue anyway
                }
            }

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

                if (originalCursorVisible.HasValue)
                {
                    try
                    {
                        Console.CursorVisible = originalCursorVisible.Value;
                    }
                    catch
                    {
                        // If cursor visibility can't be restored, continue anyway
                    }
                }
            }
        }


        public void Dispose()
        {
            if (!_disposed)
            {
                _cts.Cancel();
                _spinnerTask.Wait(1000); // Wait up to 1 second for cleanup

                // Restore original encoding
                if (_originalOutputEncoding != null)
                {
                    try
                    {
                        Console.OutputEncoding = _originalOutputEncoding;
                    }
                    catch
                    {
                        // If encoding can't be restored, continue anyway
                    }
                }

                _cts.Dispose();
                _disposed = true;
            }
        }
    }
}
