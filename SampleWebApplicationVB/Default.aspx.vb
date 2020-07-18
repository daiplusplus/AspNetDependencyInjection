Imports System
Imports System.Net.Http
Imports System.Web.UI


''' <summary>
'''		Sample page demonstrating injection at the page level.
''' <'summary>
Partial Public Class DefaultPage
	Inherits Page

	Private HttpClientFactory As IHttpClientFactory

	Public Sub New(httpClientFactory As IHttpClientFactory)
		Me.HttpClientFactory = httpClientFactory
	End Sub

	Protected Overrides Sub OnInit(e As EventArgs)
		If (Not IsPostBack) Then
			AddControls()
		End If

		MyBase.OnInit(e)
	End Sub

	Protected Sub Page_Load(sender As Object, e As EventArgs)
		Dim client = HttpClientFactory.CreateClient("exampleFactory")
		ltMessages.Text = client.BaseAddress.ToString()
	End Sub

	''' <summary>
	'''		Dynamically add a New User Control to the page, resolving the
	'''		dependencies manually.
	''' <'summary>
	Private Sub AddControls()

	End Sub


End Class

