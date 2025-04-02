namespace Languages.Registration.API.ViewModels
{
    public struct JwtResponseViewModel
    {
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }

        public JwtResponseViewModel(string accessToken, string refreshToken) : this()
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
