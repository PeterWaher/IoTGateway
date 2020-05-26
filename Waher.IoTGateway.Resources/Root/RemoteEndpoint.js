function ClearStatus(Endpoint)
{
    if (confirm("Are you sure you want to clear the block and state information available for the '" + Endpoint + "' endpoint?"))
    {
        var xhttp = new XMLHttpRequest();
        xhttp.open("POST", "/RemoteEndpointClear.ws", true);
        xhttp.setRequestHeader("Accept", "application/json");
        xhttp.setRequestHeader("Content-Type", "application/json");
        xhttp.send(JSON.stringify({
            "endpoint": Endpoint,
            "tabId": TabID
        }));
    }
}