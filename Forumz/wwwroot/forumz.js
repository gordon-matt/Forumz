function whenPickstackReady(timeoutMs) {
    const started = Date.now();
    return new Promise((resolve, reject) => {
        (function check() {
            if (window.Pickstack && window.Pickstack.files) {
                resolve(window.Pickstack.files);
            } else if (Date.now() - started > timeoutMs) {
                reject(new Error("Pickstack's script has not loaded on this page."));
            } else {
                setTimeout(check, 50);
            }
        })();
    });
}

// The choice comes back through the .NET callback. Closing the dialog without choosing never answers.
export async function pickFile(dotnet, method, type, root) {
    const files = await whenPickstackReady(5000);
    files.open({
        type: type || "any",
        root: root || "",
        callback: file => dotnet.invokeMethodAsync(method, file && file.url ? file.url : null)
    });
}
