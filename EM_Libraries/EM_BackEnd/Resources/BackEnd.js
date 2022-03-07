// This function will make a new call to EM_BackEnd using the given URL and parameters, and runs the supplied function on the response
function embe_BackEndRequest(functionToRun, URL, URLparams) {
    xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
		if (this.responseText == null || this.responseText == "") alert("Error: empty response for '"+URL+"' with '"+URLparams+"'");
		else
		{
            var response;
			try
            {
                response = JSON.parse(this.responseText);
                if (response.errorMessage != null && response.errorMessage != "") alert(response.errorMessage);
                else if (response != null) {
                    if (functionToRun != null) functionToRun(response);
				}
                else alert("error in response!" + response);
			}
			catch(e)
            {
                alert("Error parsing response: " + e.message + "\n" + this.responseText);
                console.log(this.responseText);
			}
		}
    };
    xhttp.open("POST", URL, true);
    xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
    xhttp.send(URLparams);
}

// This function is used to parse the URL parameters into a list
function embe_GetURLParameter(param) {
    var url = window.location.href;
    var question = url.indexOf("?");
    var hash = url.indexOf("#");
    if(hash==-1 && question==-1) return "";
    if(hash==-1) hash = url.length;
    var query = question==-1 || hash==question+1 ? url.substring(hash) : 
    url.substring(question+1,hash);
    var result = {};
    query.split("&").forEach(function(part) {
        if(!part) return "";
        part = part.split("+").join(" "); // replace every + with space, regexp-free version
        var eq = part.indexOf("=");
        var key = eq>-1 ? part.substr(0,eq) : part;
        var val = eq>-1 ? decodeURIComponent(part.substr(eq+1)) : "";
        var from = key.indexOf("[");
        if(from==-1) result[decodeURIComponent(key)] = val;
        else {
            var to = key.indexOf("]",from);
            var index = decodeURIComponent(key.substring(from+1,to));
            key = decodeURIComponent(key.substring(0,from));
            if(!result[key]) result[key] = [];
            if(!index) result[key].push(val);
            else result[key][index] = val;
        }
    });
    return result[param] || "";
}

function embe_GetURLCurrentFolder() {
    var url = window.location.href;
    var path = "";
    if (url.lastIndexOf("/") > -1)
        path = url.substring(0, url.lastIndexOf("/") + 1);
    return path;
}

function embe_MessageBox(content) {
    document.getElementById("embe-id-msg-box").style.display = "block";
    document.getElementById("embe-id-msg-box-content").innerHTML = content;
}

// Helper functions for the tooltips
if (typeof tlite !=='undefined')
tlite(function (el) {
    var when = classWhen(el);
    return when('tooltip', { grav: 's' }) ||
    when('tooltip-n', { grav: 'n' }) ||
    when('tooltip-s', { grav: 's' }) ||
    when('tooltip-w', { grav: 'w' }) ||
    when('tooltip-e', { grav: 'e' }) ||
    when('tooltip-se', { grav: 'se' }) ||
    when('tooltip-ne', { grav: 'ne' }) ||
    when('tooltip-sw', { grav: 'sw' }) ||
    when('tooltip-nw', { grav: 'nw' })
});

function classWhen(el) {
    var classes = (el.className || '').split(' ');
    return function (cssClass, opts) {
        return ~classes.indexOf(cssClass) && opts;
    }
}
// End of tooltips