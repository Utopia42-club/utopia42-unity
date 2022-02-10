mergeInto(LibraryManager.library, {
    isBridgePresent: function(){
        return window.bridge != null;
    },

    callOnBridge: function (fn, p) {
        var functionName = Pointer_stringify(fn);
        var parameter = JSON.parse(Pointer_stringify(p));
        var r = window.bridge[functionName](parameter);

        if (r) {
            var result = JSON.stringify(r)
            var bufferSize = lengthBytesUTF8(result) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(result, buffer, bufferSize);
            return buffer;
        }
        return null;
    },

    callAsyncOnBridge: function (cid, fn, p) {
        var functionName = Pointer_stringify(fn);
        var parameter = JSON.parse(Pointer_stringify(p));
        var id = Pointer_stringify(cid);

        window.bridge[functionName](parameter).subscribe(
            {
                next: function (r) {
                    console.log("responding to js, id: ", id, "body: ", JSON.stringify(r));
                    var result = JSON.stringify({ id: id, body: JSON.stringify(r) });
                    window.bridge.unityInstance.SendMessage('WebBridge', 'Respond', result);
                },
                error: function (err) {
                    console.error(err);
                },
                complete: function () {
                }
            });
    }
});
