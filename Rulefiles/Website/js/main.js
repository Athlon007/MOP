function download() {
    fetch(`http://athlon.kkmr.pl/mop/rulefiles.zip`)
        .then(resp => resp.blob())
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            // the filename you want
            a.download = `rulefiles.zip`;
            document.body.appendChild(a);
            a.click();
            window.URL.revokeObjectURL(url);
        })
        .catch(() => alert('Error!'));
}