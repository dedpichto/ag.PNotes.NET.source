using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Google.Contacts;
using Google.GData.Apps;
using Google.GData.Client;

namespace PNContacts
{
    /// <summary>
    /// Represents class for retrieving contacts information (full name/primary e-mail address) from Google
    /// </summary>
    public class GContacts
    {
        private const string SCOPE = "https://www.google.com/m8/feeds/";

        /// <summary>
        /// Access token
        /// </summary>
        public string AccessToken { get; set; }
        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// Access token expiration
        /// </summary>
        public DateTime TokenExpiry { get; set; }
        /// <summary>
        /// Token type
        /// </summary>
        public string TokenType { get; private set; }

        private readonly string _ClientId, _ClientSecret, _AppName;
        private OAuth2Parameters _Parameters;

        /// <summary>
        /// Creates new instance of GContacts
        /// </summary>
        /// <param name="clientId">Client Id of calling application</param>
        /// <param name="clientsecret">Client secret of calling application</param>
        /// <param name="appName">Application name</param>
        public GContacts(string clientId, string clientsecret, string appName)
        {
            _ClientId = clientId;
            _ClientSecret = clientsecret;
            _AppName = appName;
        }

        /// <summary>
        /// Authenticates client on Google server and retrieves access and refres tokens
        /// </summary>
        /// <returns>True if authentication was successfull, false otherwise</returns>
        public bool Authenticate()
        {
            try
            {
                _Parameters = new OAuth2Parameters
                    {
                        ClientId = _ClientId,
                        ClientSecret = _ClientSecret,
                        RedirectUri = Constants.REDIRECT_URL,
                        Scope = SCOPE,
                        State = Constants.AUTH_STATE,
                        ApprovalPrompt = "force"
                    };
                var url = OAuthUtil.CreateOAuth2AuthorizationUrl(_Parameters);
                var dlgAuth = new DlgAuth(url);
                if (dlgAuth.ShowDialog() != DialogResult.OK) return false;
                _Parameters.AccessCode = dlgAuth.AuthCode;
                OAuthUtil.GetAccessToken(_Parameters);

                AccessToken = _Parameters.AccessToken;
                RefreshToken = _Parameters.RefreshToken;
                TokenExpiry = _Parameters.TokenExpiry;
                TokenType = _Parameters.TokenType;

                return true;
            }
            catch (AppsException appex)
            {
                var sb = new StringBuilder("Error code: ");
                sb.Append(appex.ErrorCode);
                sb.Append("; Invalid input: ");
                sb.Append(appex.InvalidInput);
                sb.Append("; Reason: ");
                sb.Append(appex.Reason);
                sb.Append("; Response string: ");
                sb.Append(appex.ResponseString);
                throw new PNContactsException(appex) { AdditionalInfo = sb.ToString() };
            }
            catch (GDataRequestException reqex)
            {
                throw new PNContactsException(reqex) { AdditionalInfo = reqex.InnerException != null ? reqex.InnerException.Message : "" };
            }
            catch (LoggedException lex)
            {
                throw new PNContactsException(lex) { AdditionalInfo = lex.InnerException != null ? lex.InnerException.Message : "" };
            }
            catch (Exception ex)
            {
                throw new PNContactsException(ex);
            }
        }

        /// <summary>
        /// Refreshes access token
        /// </summary>
        /// <returns>True if access token has been refreshed successfully, false otherwise</returns>
        public bool RefreshAccessToken()
        {
            try
            {
                _Parameters = new OAuth2Parameters
                    {
                        ClientId = _ClientId,
                        ClientSecret = _ClientSecret,
                        AccessToken = AccessToken,
                        RefreshToken = RefreshToken
                    };
                OAuthUtil.RefreshAccessToken(_Parameters);

                AccessToken = _Parameters.AccessToken;
                if (!string.IsNullOrWhiteSpace(_Parameters.RefreshToken) && RefreshToken != _Parameters.RefreshToken)
                    RefreshToken = _Parameters.RefreshToken;
                TokenExpiry = _Parameters.TokenExpiry;
                TokenType = _Parameters.TokenType;

                return true;
            }
            catch (AppsException appex)
            {
                var sb = new StringBuilder("Error code: ");
                sb.Append(appex.ErrorCode);
                sb.Append("; Invalid input: ");
                sb.Append(appex.InvalidInput);
                sb.Append("; Reason: ");
                sb.Append(appex.Reason);
                sb.Append("; Response string: ");
                sb.Append(appex.ResponseString);
                throw new PNContactsException(appex) { AdditionalInfo = sb.ToString() };
            }
            catch (GDataRequestException reqex)
            {
                throw new PNContactsException(reqex) { AdditionalInfo = reqex.InnerException != null ? reqex.InnerException.Message : "" };
            }
            catch (LoggedException lex)
            {
                throw new PNContactsException(lex) { AdditionalInfo = lex.InnerException != null ? lex.InnerException.Message : "" };
            }
            catch (Exception ex)
            {
                throw new PNContactsException(ex);
            }
        }

        /// <summary>
        /// Gets list of full name/primary e-mail address pairs of contacts
        /// </summary>
        /// <returns>List of full name/primary e-mail address pairs of contacts, where item1 of entry represents full name and item2 - e-mail address</returns>
        public List<Tuple<string, string>> GetContacts()
        {
            try
            {
                var settings = new RequestSettings(_AppName, _Parameters) { AutoPaging = true };
                var cr = new ContactsRequest(settings);

                var f = cr.GetContacts();
                return
                    f.Entries.Where(c => c.Name != null && c.PrimaryEmail != null)
                        .Select(c => Tuple.Create(c.Name.FullName, c.PrimaryEmail.Address)).ToList();
            }
            catch (AppsException appex)
            {
                var sb = new StringBuilder("Error code: ");
                sb.Append(appex.ErrorCode);
                sb.Append("; Invalid input: ");
                sb.Append(appex.InvalidInput);
                sb.Append("; Reason: ");
                sb.Append(appex.Reason);
                sb.Append("; Response string: ");
                sb.Append(appex.ResponseString);
                throw new PNContactsException(appex) { AdditionalInfo = sb.ToString() };
            }
            catch (GDataRequestException reqex)
            {
                throw new PNContactsException(reqex) { AdditionalInfo = reqex.InnerException != null ? reqex.InnerException.Message : "" };
            }
            catch (LoggedException lex)
            {
                throw new PNContactsException(lex) { AdditionalInfo = lex.InnerException != null ? lex.InnerException.Message : "" };
            }
            catch (Exception ex)
            {
                throw new PNContactsException(ex);
            }
        }
    }
}
