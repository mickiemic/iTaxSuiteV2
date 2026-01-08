using iTaxSuite.Library.Constants;
using Microsoft.AspNetCore.DataProtection;

namespace iTaxSuite.CLIApp
{
    public class iTaxDriver
    {
        private readonly IDataProtector _dataProtector;

        public iTaxDriver(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(SecureConst.DATA_PURPOSE);
        }

        public async void RunConsoleApp()
        {
            string _method_ = "RunConsoleApp";
            try
            {
                bool loop = true;
                do
                {
                    await Console.Out.WriteAsync("========== CONSOLE ACTIONS =============" +
                            "\r\n 1. Test Data Protection" +
                            "\r\n 2. Test Data UnProtect" +
                            "\r\n 0. Exit\r\nSelect an Option: ");
                    string _input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(_input))
                    {
                        Console.WriteLine("Invalid Input [" + _input + "]! Please retry.");
                    }
                    string _choice = _input ?? string.Empty.Trim();

                    switch (_choice)
                    {
                        case "1":
                            {
                                await TestDataProtection();
                                break;
                            }
                        case "2":
                            {
                                await TestDataUnProtect();
                                break;
                            }
                        case "0":
                            loop = false;
                            break;
                    }

                } while(loop);

            }
            catch (Exception ex)
            {
                //UI.Error(ex, ex.GetBaseException().ToString());
                Console.WriteLine(ex.GetBaseException());
            }

        }

        private async Task TestDataUnProtect()
        {
            string _method_ = "TestDataProtection";
            try
            {
                await Task.FromResult(0);
                Console.Write("Enter input: ");
                string input = Console.ReadLine();

                // unprotect the payload
                string unprotectedPayload = _dataProtector.Unprotect(input);
                Console.WriteLine($"{_method_} : Unprotect returned: {unprotectedPayload}");
            }
            catch (Exception ex)
            {
                //UI.Error(ex, ex.GetBaseException().ToString());
                Console.WriteLine(ex.GetBaseException());
            }
        }

        private async Task TestDataProtection()
        {
            string _method_ = "TestDataProtection";
            try
            {
                await Task.FromResult(0);
                Console.Write("Enter input: ");
                string input = Console.ReadLine();

                // protect the payload
                string protectedPayload = _dataProtector.Protect(input);
                Console.WriteLine($"{_method_} : Protect returned: {protectedPayload}");

                // unprotect the payload
                string unprotectedPayload = _dataProtector.Unprotect(protectedPayload);
                Console.WriteLine($"{_method_} : Unprotect returned: {unprotectedPayload}");
            }
            catch (Exception ex)
            {
                //UI.Error(ex, ex.GetBaseException().ToString());
                Console.WriteLine(ex.GetBaseException());
            }
        }
    }
}
