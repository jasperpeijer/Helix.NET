export function drawDotPlot(canvasId, lines, lenA, lenB) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    const scaleX = canvas.width / lenA;
    const scaleY = canvas.height / lenB;

    ctx.strokeStyle = '#007bff';
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';

    ctx.beginPath();

    for (let i = 0; i < lines.length; i += 4) {
        let x1 = lines[i] * scaleX;
        let y1 = lines[i + 1] * scaleY;
        let x2 = lines[i + 2] * scaleX;
        let y2 = lines[i + 3] * scaleY;

        ctx.moveTo(x1, y1);
        ctx.lineTo(x2, y2);
    }

    ctx.stroke();
}