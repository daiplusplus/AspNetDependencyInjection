<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="InjectedControl.ascx.cs" Inherits="SampleWebApplication.InjectedControl" %>


<fieldset>
    <legend>Injected User Control</legend>
    
    <p>Service 1 = <%= InjectedService1.SayHello() %></p>
    <p>Service 2 = <%= InjectedService2.SayHello() %></p>
</fieldset>