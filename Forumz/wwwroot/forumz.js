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

// Inserts a quote at the caret. Returns the editor's new source, or null when the caller
// should append the quote to the bound text (the Markdown preview has no text area).
export function insertQuote(root, snippet) {
    if (!root || !snippet) {
        return null;
    }

    let written = null;
    try {
        const area = root.querySelector("textarea.nf-markdown-input");
        if (area) {
            written = insertAtCaret(area, snippet);
            area.focus();
        } else {
            const host = root.querySelector("textarea.pickstack-editor");
            const editor = host && host._pickstackEditor;
            if (editor && editor.initialized !== false && typeof editor.insertContent === "function") {
                editor.focus();
                editor.insertContent(snippet);
                written = typeof editor.getContent === "function" ? editor.getContent() : snippet;
            }
        }
    } catch {
        written = null;
    }

    root.scrollIntoView({ behavior: "smooth", block: "center" });
    return written;
}

function insertAtCaret(area, snippet) {
    const start = area.selectionStart ?? area.value.length;
    const end = area.selectionEnd ?? start;
    const value = area.value;
    const before = value.slice(0, start);
    const after = value.slice(end);
    const piece = padQuote(before, snippet, after);
    const next = before + piece + after;
    area.value = next;
    area.setSelectionRange(before.length + piece.length, before.length + piece.length);
    return next;
}

function padQuote(before, snippet, after) {
    const lead = before.length > 0 && !before.endsWith("\n\n")
        ? (before.endsWith("\n") ? "\n" : "\n\n")
        : "";
    const separated = snippet.endsWith("\n\n") || after.startsWith("\n\n");
    const trail = after.length > 0 && !separated
        ? (snippet.endsWith("\n") || after.startsWith("\n") ? "\n" : "\n\n")
        : "";
    return lead + snippet + trail;
}

// Opens the browser's own file dialog. Clearing the value lets the same file be chosen again.
export function chooseFile(input) {
    if (!input || typeof input.click !== "function") {
        return;
    }

    input.value = "";
    input.click();
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
