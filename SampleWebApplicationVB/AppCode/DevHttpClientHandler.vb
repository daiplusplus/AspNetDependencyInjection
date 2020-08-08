Imports System
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks

Public Class DevHttpClientHandler
    Inherits HttpClientHandler


    Public Sub New()

    End Sub

    Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        If cancellationToken.IsCancellationRequested Then cancellationToken.ThrowIfCancellationRequested()
        Dim response = New HttpResponseMessage(Net.HttpStatusCode.OK) With {
            .Content = New StringContent("Greetings, from Developer land!")
        }
        Return Task.FromResult(response)
    End Function
End Class
