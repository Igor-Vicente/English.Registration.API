namespace Languages.Registration.API.ViewModels
{
    public class JwtResponseViewModel
    {
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public ResponseUserViewModel? User { get; private set; }

        public JwtResponseViewModel(string accessToken, string refreshToken, ResponseUserViewModel? user = null)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            User = user;
        }
    }
}
