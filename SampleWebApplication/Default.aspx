<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
	CodeBehind="Default.aspx.cs" Inherits="SampleWebApplication._Default" %>

<%@ Register src="InjectedControl.ascx" tagName="InjectedControl" tagPrefix="UC" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
	<h2>Welcome to Injected ASP.NET WebForms using Unity!</h2>
	<p>
		To learn more about Unity.WebForms, visit <a href="https://bitbucket.org/KyleK/unity.webforms" target="_blank" title="Unity.WebForms Website">https://bitbucket.org/KyleK/unity.webforms</a>.
	</p>
	
	<fieldset>
		<legend>Injected Page</legend>
		
		<p>Service 1: <%= InjectedService1.SayHello() %></p>
		<p>Service 2: <%= InjectedService2.SayHello() %></p>
	</fieldset>
	
	<UC:InjectedControl runat="server" ID="InjectedUC" />
	
	<fieldset>
		<legend>Dynamically generated control; resolved manually</legend>
		
		<asp:PlaceHolder runat="server" ID="DynamicInjectedControl" />
	</fieldset>
</asp:Content>
