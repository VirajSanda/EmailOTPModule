namespace EmailOTPModule.Tests
{
    public class EmailOTPModuleTests
    {
        [Fact]
        public async Task Generate_OTP_ValidEmail()
        {
            // Arrange
            var emailModule = new EmailOTPModule();
            emailModule.Start();

            // Act
            var result = await emailModule.Generate_OTP_Email("test@dso.org.sg");

            // Assert
            Assert.Equal(OTPStatus.STATUS_EMAIL_OK, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("test@gmail.com")]
        [InlineData("test@dso.com")]
        public async Task Generate_OTP_InvalidEmail(string email)
        {
            // Arrange
            var emailModule = new EmailOTPModule();
            emailModule.Start();

            // Act
            var result = await emailModule.Generate_OTP_Email(email);

            // Assert
            Assert.Equal(OTPStatus.STATUS_EMAIL_INVALID, result);
        }

        //[Fact]
        //public async Task CheckOTP_CorrectOTP()
        //{
        //    // Arrange
        //    var emailModule = new EmailOTPModule();
        //    emailModule.Start();
        //    await emailModule.Generate_OTP_Email("test@dso.org.sg");

        //    // Create mock input with correct OTP
        //    var mockInput = new InputStreamService("123456");

        //    // Act
        //    var result = await emailModule.CheckOTP(mockInput);

        //    // Assert
        //    Assert.Equal(OTPStatus.STATUS_OTP_OK, result);
        //}

        [Fact]
        public async Task CheckOTP_TooManyAttempts()
        {
            // Arrange
            var emailModule = new EmailOTPModule();
            emailModule.Start();
            await emailModule.Generate_OTP_Email("test@dso.org.sg");

            // Create mock input with 11 wrong attempts
            var mockInput = new InputStreamService(
                "000000", "111111", "222222", "333333", "444444",
                "555555", "666666", "777777", "888888", "999999"
            );

            // Act
            var result = await emailModule.CheckOTP(mockInput);

            // Assert
            Assert.Equal(OTPStatus.STATUS_OTP_FAIL, result);
        }

        [Fact]
        public void Start_CalledTwice_()
        {
            // Arrange
            var emailModule = new EmailOTPModule();
            emailModule.Start();

            // Act & Assert
            Assert.Throws<EmailOTPException>(() => emailModule.Start());
        }

        [Fact]
        public void Close_NotStarted()
        {
            // Arrange
            var emailModule = new EmailOTPModule();

            // Act & Assert
            Assert.Throws<EmailOTPException>(() => emailModule.Close());
        }
    }
}