using System.Text.RegularExpressions;

namespace EmailOTPModule
{

    public class EmailOTPModule : IDisposable
    {
        #region private veriables
        private readonly IEmailSenderService _emailSender;
        private string? currentOTP;
        private DateTime otpGenTime;
        private const int TRY_COUNT = 10;
        private const int OTP_TIMEOUT_SECONDS = 60;
        private const string ALLOWED_DOMAIN = "@dso.org.sg";
        private bool isStarted = false;
        private readonly Random _random;
        #endregion

        public EmailOTPModule(IEmailSenderService? emailSender = null)
        {
            _emailSender = emailSender ?? new EmailSenderService();
            _random = new Random();
        }

        /// <summary>
        /// module start
        /// </summary>
        /// <exception cref="EmailOTPException"></exception>
        public void Start()
        {
            if (isStarted)
            {
                throw new EmailOTPException("Module already started");
            }
            isStarted = true;
            currentOTP = null;
        }

        /// <summary>
        /// module close
        /// </summary>
        /// <exception cref="EmailOTPException"></exception>
        public void Close()
        {
            if (!isStarted)
            {
                throw new EmailOTPException("Module not started");
            }

            isStarted = false;
            currentOTP = null;
        }

        /// <summary>
        /// generate random 6 digit OTP
        /// </summary>
        /// <returns></returns>
        private string GenerateOTP()
        {
            return _random.Next(0, 999999).ToString("D6");
        }

        /// <summary>
        /// validate email and domain specific email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                // Basic email validation
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!regex.IsMatch(email))
                {
                    return false;
                }

                // Check domain
                return email.EndsWith(ALLOWED_DOMAIN, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// generate OTP
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        /// <exception cref="EmailOTPException"></exception>
        public async Task<OTPStatus> Generate_OTP_Email(string userEmail)
        {
            if (!isStarted)
            {
                throw new EmailOTPException("Module not started");
            }

            if (!IsEmailValid(userEmail))
            {
                return OTPStatus.STATUS_EMAIL_INVALID;
            }

            try
            {
                currentOTP = GenerateOTP();
                otpGenTime = DateTime.UtcNow;

                string emailBody = $"Your OTP Code is {currentOTP}. The code is valid for 1 minute";
                bool emailSent = await _emailSender.SendEmailAsync(userEmail, emailBody);

                return emailSent ? OTPStatus.STATUS_EMAIL_OK : OTPStatus.STATUS_EMAIL_FAIL;
            }
            catch
            {
                return OTPStatus.STATUS_EMAIL_FAIL;
            }
        }

        /// <summary>
        /// check OTP
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="EmailOTPException"></exception>
        public async Task<OTPStatus> CheckOTP(IInputStreamService input)
        {
            if (!isStarted)
            {
                throw new EmailOTPException("Module not started");
            }

            if (string.IsNullOrEmpty(currentOTP))
            {
                throw new EmailOTPException("OTP has not been generated");
            }

            int tries = 0;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(OTP_TIMEOUT_SECONDS));

            try
            {
                while (tries < TRY_COUNT)
                {
                    try
                    {
                        string userInput = await input.ReadOTPAsync(cts.Token);
                        tries++;

                        // Check if OTP has expired
                        if ((DateTime.UtcNow - otpGenTime).TotalSeconds > OTP_TIMEOUT_SECONDS)
                        {
                            return OTPStatus.STATUS_OTP_TIMEOUT;
                        }
                        // Check OTP
                        if (userInput == currentOTP)
                        {
                            return OTPStatus.STATUS_OTP_OK;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return OTPStatus.STATUS_OTP_TIMEOUT;
                    }
                }

                return OTPStatus.STATUS_OTP_FAIL;
            }
            catch (OperationCanceledException)
            {
                return OTPStatus.STATUS_OTP_TIMEOUT;
            }
        }

        public void Dispose()
        {
            if (isStarted)
            {
                Close();
            }
        }
    }


    public class EmailOTPException : Exception
    {
        public EmailOTPException(string message) : base(message) { }
    }

    public enum OTPStatus
    {
        STATUS_EMAIL_OK,
        STATUS_EMAIL_FAIL,
        STATUS_EMAIL_INVALID,
        STATUS_OTP_OK,
        STATUS_OTP_FAIL,
        STATUS_OTP_TIMEOUT
    }

    public interface IEmailSenderService
    {
        Task<bool> SendEmailAsync(string emailAddress, string emailBody);
    }


    public class EmailSenderService : IEmailSenderService
    {
        public async Task<bool> SendEmailAsync(string emailAddress, string emailBody)
        {
            // Email sending delay
            await Task.Delay(100);
            return true;
        }
    }

    public interface IInputStreamService
    {
        Task<string> ReadOTPAsync(CancellationToken cancellationToken);
    }

    public class InputStreamService : IInputStreamService
    {
        private readonly string[] _inputs;
        private int _currentIndex;

        public InputStreamService(params string[] inputs)
        {
            _inputs = inputs;
            _currentIndex = 0;
        }

        public async Task<string> ReadOTPAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken); // Simulate input delay
            if (_currentIndex >= _inputs.Length)
            {
                return string.Empty;
            }
            return _inputs[_currentIndex++];
        }
    }
}