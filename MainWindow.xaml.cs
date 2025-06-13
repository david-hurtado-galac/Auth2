using System.Windows;
using Microsoft.Identity.Client;
using Microsoft.Graph;
using System.IO;
using Microsoft.Win32;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Graph.Models;

namespace Auth2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private IPublicClientApplication? _pca;
    private GraphServiceClient? _graphClient;
    private readonly string[] _scopes = new[] { "User.Read", "Mail.Send", "Files.ReadWrite", "offline_access" };
    private readonly string _clientId = "<Azure:CLIENT_ID>"; // Reemplaza por tu ClientId de Azure
    private readonly string _tokenPath = "token.cache";

    public MainWindow()
    {
        InitializeComponent();
        InitAuth();
    }

    private async Task<AuthenticationResult?> AcquireTokenAsync()
    {
        if (_pca == null) return null;
        var accounts = await _pca.GetAccountsAsync();
        try
        {
            // Intenta obtener el token silenciosamente
            return await _pca.AcquireTokenSilent(_scopes, accounts.FirstOrDefault()).ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            // Si no es posible, solicita interacción
            return await _pca.AcquireTokenInteractive(_scopes).ExecuteAsync();
        }
    }

    private void InitAuth()
    {
        _pca = PublicClientApplicationBuilder.Create(_clientId)
            .WithRedirectUri("http://localhost")
            .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
            .Build();
        var authProvider = new MsalAuthProvider(_pca, _scopes, AcquireTokenAsync);
        _graphClient = new GraphServiceClient(authProvider);
    }

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        if (_pca == null || _graphClient == null) return;
        try
        {
            var accounts = await _pca.GetAccountsAsync();
            foreach (var acc in accounts) await _pca.RemoveAsync(acc);
            await _graphClient.Me.GetAsync();
            LblStatus.Text = "Sesión iniciada correctamente.";
        }
        catch (Exception ex)
        {
            LblStatus.Text = "Error de autenticación: " + ex.Message;
        }
    }

    private async void BtnSend_Click(object sender, RoutedEventArgs e)
    {
        if (_graphClient == null) return;
        try
        {
            var message = new Microsoft.Graph.Models.Message
            {
                Subject = TxtSubject.Text,
                Body = new Microsoft.Graph.Models.ItemBody { ContentType = Microsoft.Graph.Models.BodyType.Text, Content = TxtBody.Text },
                ToRecipients = new List<Microsoft.Graph.Models.Recipient> { new Microsoft.Graph.Models.Recipient { EmailAddress = new Microsoft.Graph.Models.EmailAddress { Address = TxtTo.Text } } }
            };
            var sendMailRequest = new Microsoft.Graph.Me.SendMail.SendMailPostRequestBody { Message = message };
            await _graphClient.Me.SendMail.PostAsync(sendMailRequest);
            LblStatus.Text = "Correo enviado correctamente.";
        }
        catch (Exception ex)
        {
            LblStatus.Text = "Error al enviar correo: " + ex.Message;
        }
    }

    private async void BtnUpload_Click(object sender, RoutedEventArgs e)
    {
        if (_graphClient == null) return;
        var dlg = new OpenFileDialog();
        if (dlg.ShowDialog() == true)
        {
            try
            {
                using var stream = File.OpenRead(dlg.FileName);
                var fileName = System.IO.Path.GetFileName(dlg.FileName);
                // Subir archivo a la raíz de OneDrive usando la sintaxis correcta de Microsoft.Graph 5.x
                var requestInfo = new RequestInformation {
                    HttpMethod = Method.PUT,
                    UrlTemplate = "https://graph.microsoft.com/v1.0/me/drive/root:/" + fileName + ":/content"
                };
                requestInfo.SetStreamContent(stream, "application/octet-stream");
                await _graphClient.RequestAdapter.SendAsync<DriveItem>(requestInfo, DriveItem.CreateFromDiscriminatorValue, null, default);
                LblStatus.Text = "Archivo subido a OneDrive.";
            }
            catch (Exception ex)
            {
                LblStatus.Text = "Error al subir archivo: " + ex.Message;
            }
        }
    }
}

public class MsalAuthProvider : IAuthenticationProvider {
    private readonly IPublicClientApplication _pca;
    private readonly string[] _scopes;
    private readonly Func<Task<AuthenticationResult?>> _acquireTokenAsync;
    public MsalAuthProvider(IPublicClientApplication pca, string[] scopes, Func<Task<AuthenticationResult?>> acquireTokenAsync) {
        _pca = pca;
        _scopes = scopes;
        _acquireTokenAsync = acquireTokenAsync;
    }
    public async Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default) {
        var result = await _acquireTokenAsync();
        if (result != null)
            request.Headers["Authorization"] = new[] { $"Bearer {result.AccessToken}" };
    }
}