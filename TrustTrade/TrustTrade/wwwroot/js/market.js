function renderSparklineCharts() {
    const cards = document.querySelectorAll('.stock-card');

    cards.forEach(card => {
        const canvas = card.querySelector('canvas');
        if (!canvas) return;

        const ctx = canvas.getContext('2d');
        const highsRaw = card.getAttribute('data-highs');
        if (!highsRaw) return;

        const prices = JSON.parse(highsRaw).slice(-3);

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: prices.map((_, i) => `Day ${i + 1}`),
                datasets: [{
                    data: prices,
                    borderColor: '#0d6efd',
                    backgroundColor: 'rgba(13,110,253,0.2)',
                    borderWidth: 1.5,
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

let stockChart;
let modalHighs = [];
let modalDates = [];

window.openStockModal = async function (ticker) {
    document.getElementById("modalTicker").innerText = ticker;

    try {
        const response = await fetch(`/api/market/highlow?ticker=${ticker}`);
        const data = await response.json();

        modalHighs = data.map(d => d.high);
        modalDates = data.map(d => d.date);

        renderModalChart(3); // default

        document.querySelectorAll('#historyFilter .chart-filter-button').forEach(btn => {
            btn.onclick = () => {
                document.querySelectorAll('#historyFilter .chart-filter-button')
                    .forEach(b => b.classList.remove("active"));
                btn.classList.add("active");

                const days = parseInt(btn.getAttribute("data-days"));
                renderModalChart(days);
            };
        });

        new bootstrap.Modal(document.getElementById("stockModal")).show();
    } catch (err) {
        console.error("Chart load failed", err);
    }
};

function renderModalChart(days) {
    const labels = modalDates.slice(-days);
    const points = modalHighs.slice(-days);

    const canvas = document.getElementById("stockChart");
    const ctx = canvas.getContext("2d");

    if (stockChart) stockChart.destroy();

    const minPrice = Math.min(...points);
    const maxPrice = Math.max(...points);
    const padding = (maxPrice - minPrice) * 0.15;

    stockChart = new Chart(ctx, {
        type: "line",
        data: {
            labels,
            datasets: [{
                label: `High Price (Last ${days} Days)`,
                data: points,
                borderColor: "lime",
                backgroundColor: "rgba(0,255,0,0.1)",
                tension: 0.4,
                pointRadius: 3
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { display: true }
            },
            scales: {
                x: { display: true },
                y: {
                    min: minPrice - padding,
                    max: maxPrice + padding,
                    ticks: {
                        callback: val => `$${val.toFixed(2)}`
                    }
                }
            }
        }
    });
}

document.getElementById("stockBtn").addEventListener("click", () => loadMarketData("stock"));
document.getElementById("cryptoBtn").addEventListener("click", () => loadMarketData("crypto"));

async function loadMarketData(type) {
    try {
        const response = await fetch(`/Market/SearchStocks?searchTerm=&type=${type}`);
        const html = await response.text();

        document.getElementById("marketCards").innerHTML = html;
        document.getElementById("marketTitle").textContent =
            type === "crypto" ? "Top Cryptocurrencies" : "Top Stocks";

        document.getElementById("stockBtn").classList.toggle("active", type === "stock");
        document.getElementById("cryptoBtn").classList.toggle("active", type === "crypto");

        renderSparklineCharts();
    } catch (err) {
        console.error("Failed to load market data:", err);
    }
}

document.getElementById("stockSearchInput").addEventListener("input", async function () {
    const term = this.value.trim();
    const isCrypto = document.getElementById("cryptoBtn").classList.contains("active");
    const type = isCrypto ? "crypto" : "stock";

    if (term.length < 1) {
        loadMarketData(type);
        return;
    }

    try {
        const response = await fetch(`/Market/SearchStocks?searchTerm=${encodeURIComponent(term)}&type=${type}`);
        const html = await response.text();
        document.getElementById("marketCards").innerHTML = html;

        renderSparklineCharts();
    } catch (error) {
        console.error("Search failed:", error);
    }
});

document.addEventListener('DOMContentLoaded', renderSparklineCharts);
