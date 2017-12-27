//require("knockout-3.4.2.js");
//console.log("script-0");

var CCMAPageCore = function () {
    var self = this;
    self.APIServer = "http://localhost:52071/api/ccmadb/";
    self.docs = ko.observableArray();

    self.fetch = function (path) {
        var path = path || "";
        return new Promise(function (resolve, reject) {
            var xhr = new XMLHttpRequest();
            xhr.open("GET", (self.APIServer + path), true);
            xhr.onload = function () {
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(xhr.response);
                } else {
                    reject({
                        status: xhr.status,
                        statusText: xhr.statusText
                    });
                }
            };
            xhr.onerror = function () {
                reject({
                    status: xhr.status,
                    statusText: xhr.statusText});
            }
            xhr.send();
        });
        
    }
    self.activeDocument = ko.observable();
    self.activeDocumentType = ko.observable();
    self.searchText = ko.observable("");
    self.searchResults = ko.observableArray([]);
    self.searchFn = function () {
        var ans = self.fetch("search/" + self.searchText());
        ans.then(function (resp) {
            var resp = JSON.parse(resp);
            //console.log(resp);
            self.searchResults.removeAll();
            self.searchResults(resp);
            console.log(self.searchResults, self.searchResults().length);
        })
    }
    self.showApplication = function (data, evt) {
        console.log("show ", data);
        var ans = self.fetch("unid/" + data.DocumentUNID);
        ans.then(function (resp) {
            var resp = JSON.parse(resp);
            var doc = {};
            for (var f = 0; f < resp.length; f++) {
                doc[resp[f].FieldName] = resp[f].FieldValue;
            } 
            self.docs.push(doc);
            console.log(doc);
            
            self.activeDocumentType(data.FormName == "frmResponse" ||doc.FormName == "frmResponse" ? "renderDiscussion" : "ApplicationRequest");
            self.activeDocument(doc);
            
            
        });
    }
    self.showApp = function (data, evt) {
        console.log("waz", data.ApplicationNumber);
        var ans = self.fetch("application/" + data.ApplicationNumber);
        ans.then(function (resp) {
            var resp = JSON.parse(resp);
            var doc = {};
            for (var f = 0; f < resp.length; f++) {
                doc[resp[f].FieldName] = resp[f].FieldValue;
            }
            self.docs.push(doc);
            console.log(doc);

            self.activeDocumentType(data.FormName == "frmResponse" || doc.FormName == "frmResponse" ? "renderDiscussion" : "ApplicationRequest");
            self.activeDocument(doc);
        });
    }
}

document.onreadystatechange = function (evt) {
    if (document.readyState == "complete") {
        var Model = new CCMAPageCore();
        ko.applyBindings(Model, document.body);
    }
}
