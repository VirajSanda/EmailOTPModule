using EmailOTPModule;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Email OTP Module");
        Console.WriteLine("--------------------");

        using (var otpModule = new EmailOTPModule.EmailOTPModule())
        {
            try
            {
                // Start the module
                otpModule.Start();

                while (true)
                {
                    Console.WriteLine("\nChoose an option:");
                    Console.WriteLine("1. Generate OTP");
                    Console.WriteLine("2. Verify OTP");
                    Console.WriteLine("3. Exit");
                    Console.Write("\nOption: ");

                    var option = Console.ReadLine();

                    switch (option)
                    {
                        case "1":
                            await GenerateOTP(otpModule);
                            break;
                        case "2":
                            await VerifyOTP(otpModule);
                            break;
                        case "3":
                            return;
                        default:
                            Console.WriteLine("Invalid option");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Generate OTP
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    static async Task GenerateOTP(EmailOTPModule.EmailOTPModule module)
    {
        Console.Write("Enter email address: ");
        var email = Console.ReadLine();

        var result = await module.Generate_OTP_Email(email);

        switch (result)
        {
            case OTPStatus.STATUS_EMAIL_OK:
                Console.WriteLine("OTP sent successfully! Check your email.");
                break;
            case OTPStatus.STATUS_EMAIL_INVALID:
                Console.WriteLine("Invalid email address! Must be from dso.org.sg domain.");
                break;
            case OTPStatus.STATUS_EMAIL_FAIL:
                Console.WriteLine("Failed to send email!");
                break;
        }
    }

    /// <summary>
    /// Verify OTP
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    static async Task VerifyOTP(EmailOTPModule.EmailOTPModule module)
    {
        // Create a custom input stream that reads from console
        var inputStream = new ConsoleInputStream();

        Console.WriteLine("Enter OTP (you have 1 minute and 10 tries):");
        var result = await module.CheckOTP(inputStream);

        switch (result)
        {
            case OTPStatus.STATUS_OTP_OK:
                Console.WriteLine("OTP verified successfully!");
                break;
            case OTPStatus.STATUS_OTP_FAIL:
                Console.WriteLine("Maximum attempts reached. Please generate a new OTP.");
                break;
            case OTPStatus.STATUS_OTP_TIMEOUT:
                Console.WriteLine("OTP verification timeout. Please generate a new OTP.");
                break;
        }
    }
}

// Implementation of IInputStreamService that reads from console
public class ConsoleInputStream : IInputStreamService
{
    /// <summary>
    /// read otp from console lines
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> ReadOTPAsync(CancellationToken cancellationToken)
    {
        // Create a task that reads from console
        var readTask = Task.Run(() => Console.ReadLine(), cancellationToken);

        try
        {
            return await readTask;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }
}