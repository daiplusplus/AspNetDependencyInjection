Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.Security
Imports System.Web.SessionState


Public Class SampleWebApplicationVBHttpApplication
    Inherits HttpApplication

    Private Shared _idSeed As Int32

    Private ReadOnly id As Int32 = System.Threading.Interlocked.Increment(_idSeed)

    Public Sub New()
    End Sub

    Sub Application_Start(sender As Object, e As EventArgs)
        '' Code that runs on application startup
    End Sub

    Sub Application_End(sender As Object, e As EventArgs)
        ''  Code that runs on application shutdown
    End Sub

    Sub Application_Error(sender As Object, e As EventArgs)
        '' Code that runs when an unhandled error occurs
    End Sub

    Sub Session_Start(sender As Object, e As EventArgs)
        '' Code that runs when a new session is started
    End Sub

    Sub Session_End(sender As Object, e As EventArgs)
        '' Code that runs when a session ends. 
        '' Note: The Session_End event is raised only when the sessionstate mode
        '' is set to InProc in the Web.config file. If session mode is set to StateServer 
        '' or SQLServer, the event is not raised.
    End Sub

End Class

