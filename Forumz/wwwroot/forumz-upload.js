export async function upload(input, url, field) {
    const file = input && input.files ? input.files[0] : null;
    if (!file) {
        return { ok: false, message: "Choose an image." };
    }

    const body = new FormData();
    body.append(field || "photo", file);
    const response = await fetch(url, { method: "POST", body, credentials: "same-origin" });
    if (!response.ok) {
        let message = "The photo could not be uploaded. Try a smaller image.";
        try {
            const problem = await response.json();
            if (problem && problem.detail) {
                message = problem.detail;
            }
        } catch {
            // The response was not JSON. The fallback message is enough.
        }

        return { ok: false, message };
    }

    const saved = await response.json();
    const address = saved.url || saved.photoUri || "";
    return { ok: true, photoUri: address, url: address };
}
