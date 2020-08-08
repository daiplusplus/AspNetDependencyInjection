Imports System
Imports System.Net.Http
Imports System.Diagnostics
Imports System.Runtime

Imports Microsoft.Extensions.DependencyInjection

Imports AspNetDependencyInjection
Imports WebActivatorEx

<Assembly: PreApplicationStartMethod(GetType(SampleApplicationStartVB), NameOf(SampleApplicationStartVB.PreStart))>
<Assembly: PostApplicationStartMethod(GetType(SampleApplicationStartVB), NameOf(SampleApplicationStartVB.PostStart))>
<Assembly: ApplicationShutdownMethod(GetType(SampleApplicationStartVB), NameOf(SampleApplicationStartVB.ApplicationShutdown))>

''' <summary>Startup class for the AspNetDependencyInjection NuGet package.</summary>
Friend Module SampleApplicationStartVB

    Private _di As ApplicationDependencyInjection
    ''' <summary>Invoked when the ASP.NET application starts up, before Global's Application_Start method runs. Dependency-injection should be configured here.</summary>
    Public Sub PreStart()
        Debug.WriteLine(NameOf(SampleApplicationStartVB) & "." & NameOf(PreStart) & "() called.")

        _di = New ApplicationDependencyInjectionBuilder().
                    ConfigureServices(AddressOf ConfigureServices).
                    Build()
    End Sub

    Private Sub ConfigureServices(services As IServiceCollection)
        ' TODO: Add any dependencies needed here
        services = services.AddDefaultHttpContextAccessor()

        Dim devMode As Boolean = True
        Dim apiBase As String = "https://github.com/Jehoel/AspNetDependencyInjection"

        services.AddHttpClient("examplefactory", Sub(c As HttpClient)
                                                     c.BaseAddress = New Uri(apiBase)
                                                 End Sub)

        services.AddHttpClient("exampleFactory", Sub(c As HttpClient)
                                                     c.BaseAddress = New Uri(apiBase)
                                                     'c.DefaultRequestHeaders.Authorization = New System.Net.Http.Headers.AuthenticationHeaderValue("Basic", $"")
                                                 End Sub).
            ConfigurePrimaryHttpMessageHandler(Function(provider) As System.Net.Http.HttpMessageHandler
                                                   If (Not devMode) Then Return New HttpClientHandler()

                                                   Return CType(New DevHttpClientHandler, HttpMessageHandler)
                                               End Function)
    End Sub


    ''' <summary>Invoked at the end of ASP.NET application start-up, after Global's Application_Start method runs. Dependency-injection re-configuration may be called here if you have services that depend on Global being initialized.</summary>
    Public Sub PostStart()
        Debug.WriteLine(NameOf(SampleApplicationStartVB) & "." & NameOf(PostStart) & "() called.")
    End Sub

    Public Sub ApplicationShutdown()
        Debug.WriteLine(NameOf(SampleApplicationStartVB) & "." & NameOf(ApplicationShutdown) & "() called.")
    End Sub
End Module
