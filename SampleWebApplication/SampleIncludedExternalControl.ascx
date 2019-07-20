<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SampleIncludedExternalControl.ascx.cs" Inherits="IncludedNamespace.SampleWebApplication.SampleIncludedExternalControl" %>

<fieldset>
    <legend>External User Control - Included (Services should not be null)</legend>
    
    <p>Service1 is <strong><asp:Literal runat="server" ID="serviceOne" /></strong></p>
    <p>Service1 is <strong><asp:Literal runat="server" ID="serviceTwo" /></strong></p>
</fieldset>