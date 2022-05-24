//See https://aka.ms/new-console-template for more information
var dc = new DesktopChanger.DesktopChanger();
var ct = new CancellationTokenSource();
dc.Run(ct.Token);
