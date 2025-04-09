// Render mini chart on each card (mocked data)
function renderSparklineCharts() {
    const cards = document.querySelectorAll('.stock-card');

    cards.forEach(card => {
        const canvas = card.querySelector('canvas');
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const ticker = card.querySelector('.card-title').textContent.trim();

        // Generate mock data
        let base = 100 + Math.random() * 50;
        const prices = [];
        for (let i = 0; i < 20; i++) {
            base += (Math.random() - 0.5) * 2;
            prices.push(Number(base.toFixed(2)));
        }

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: prices.map((_, i) => i),
                datasets: [{
                    data: prices,
                    borderColor: '#888',
                    backgroundColor: 'transparent',
                    borderWidth: 1,
                    pointRadius: 0,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: {
                    x: { display: false },
                    y: { display: false }
                }
            }
        });
    });
}

// Global modal chart instance
let stockChart;

// Expose function globally for Razor onclick
window.openStockModal = async function (ticker) {
    document.getElementById("modalTicker").innerText = ticker;

    try {
        const response = await fetch(`/api/market/history?ticker=${ticker}`);
        const data = await response.json();

        waitForChartCanvas(() => {
            const canvas = document.getElementById('stockChart');
            if (!canvas) {
                console.error("Canvas not found in modal.");
                return;
            }

            const ctx = canvas.getContext('2d');
            if (stockChart) stockChart.destroy();

            stockChart = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: data.map(p => p.time),
                    datasets: [{
                        label: `${ticker} Price`,
                        data: data.map(p => p.price),
                        borderColor: '#32cd32',
                        tension: 0.3,
                        fill: false
                    }]
                },
                options: {
                    responsive: true,
                    plugins: { legend: { display: false } },
                    scales: {
                        x: { display: true },
                        y: { display: true }
                    }
                }
            });

            const modal = new bootstrap.Modal(document.getElementById("stockModal"));
            modal.show();
        });

    } catch (error) {
        console.error("Error loading chart data:", error);
    }
};

// Wait for canvas to be in the DOM before rendering the chart
function waitForChartCanvas(callback, tries = 10) {
    const interval = setInterval(() => {
        const canvas = document.getElementById('stockChart');
        if (canvas || tries <= 0) {
            clearInterval(interval);
            callback();
        } else {
            tries--;
        }
    }, 100);
}

// Render sparklines on page load
document.addEventListener('DOMContentLoaded', renderSparklineCharts);


document.getElementById("stockBtn").addEventListener("click", () => loadMarketData("stock"));
document.getElementById("cryptoBtn").addEventListener("click", () => loadMarketData("crypto"));

async function loadMarketData(type) {
    try {
        const response = await fetch(`/Market/Index?type=${type}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const html = await response.text();

        document.getElementById("marketCards").innerHTML = html;
        document.getElementById("marketTitle").textContent =
            type === "crypto" ? "Top Cryptocurrencies" : "Top Stocks";

        document.getElementById("stockBtn").classList.toggle("active", type === "stock");
        document.getElementById("cryptoBtn").classList.toggle("active", type === "crypto");

        renderSparklineCharts(); // Re-render charts after replacing DOM
    } catch (err) {
        console.error("Failed to load market data:", err);
    }
}