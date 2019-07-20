<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SampleWebApplication.DefaultPage" %>

<%@ Register src="InjectedControl.ascx" tagName="InjectedControl" tagPrefix="UC" %>
<%@ Register src="SampleExcludedExternalControl.ascx" tagName="SampleExcludedExternalControl" tagPrefix="UC" %>
<%@ Register src="SampleIncludedExternalControl.ascx" tagName="SampleIncludedExternalControl" tagPrefix="UC" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
	<h2>Welcome to Injected ASP.NET WebForms using Unity!</h2>
	<p>
		To learn more about Unity.WebForms, visit <a href="https://bitbucket.org/KyleK/unity.webforms" target="_blank" title="Unity.WebForms Website">https://bitbucket.org/KyleK/unity.webforms</a>.
	</p>
	
	<hr />

	<fieldset>
		<legend>Request-lifetime service</legend>

		<p><code><%: this.InjectedService3.ToString() %></code></p>

	</fieldset>

	<hr />

	<fieldset>
		<legend>Injected Page</legend>
		
		<p>Service 1: <%: this.InjectedService1.SayHello() %></p>
		<p>Service 2: <%: this.InjectedService2.SayHello() %></p>
	</fieldset>
	
	<hr />

	<UC:InjectedControl runat="server" ID="InjectedUC" />

	<hr />

	<UC:SampleExcludedExternalControl runat="server" ID="SampleExcludedExternalControl" />

	<hr />

	<UC:SampleIncludedExternalControl runat="server" ID="SampleIncludedExternalControl" />
	
	<hr />

	<fieldset>
		<legend>Dynamically generated control; resolved manually</legend>
		
		<asp:PlaceHolder runat="server" ID="DynamicInjectedControl" />
	</fieldset>

	<hr />

</asp:Content>
