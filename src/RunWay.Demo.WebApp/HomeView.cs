namespace Kododo.RunWay.Demo.WebApp;

static class HomeView
{
    public static string Render(string basePath) => $$"""
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="UTF-8"/>
          <meta name="viewport" content="width=device-width,initial-scale=1"/>
          <title>RunWay Demo</title>
          <style>
            *{box-sizing:border-box;margin:0;padding:0}
            body{font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;background:#f9fafb;color:#111827;min-height:100vh}
            .hero{background:#fff;border-bottom:1px solid #e5e7eb;padding:48px 24px 40px;text-align:center}
            .hero h1{font-size:2rem;font-weight:700;margin-bottom:12px}
            .hero p{color:#6b7280;font-size:1.05rem;max-width:560px;margin:0 auto 28px}
            .btn{display:inline-flex;align-items:center;gap:8px;background:#2563eb;color:#fff;font-weight:600;font-size:1rem;padding:13px 28px;border-radius:8px;text-decoration:none;transition:background .15s}
            .btn:hover{background:#1d4ed8}
            .btn svg{flex-shrink:0}
            .note{margin-top:14px;font-size:.85rem;color:#9ca3af}
            .section-title{font-size:.75rem;font-weight:600;text-transform:uppercase;letter-spacing:.08em;color:#9ca3af;padding:32px 24px 12px;max-width:1200px;margin:0 auto}
            .grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(300px,1fr));gap:20px;padding:0 24px 32px;max-width:1200px;margin:0 auto}
            .card{background:#fff;border:1px solid #e5e7eb;border-radius:10px;padding:22px 24px;display:flex;flex-direction:column;gap:12px}
            .card-header{display:flex;align-items:flex-start;gap:12px}
            .card-icon{width:36px;height:36px;border-radius:8px;display:flex;align-items:center;justify-content:center;flex-shrink:0}
            .card-title{font-size:.95rem;font-weight:700;color:#111827;line-height:1.3}
            .card-desc{font-size:.85rem;color:#6b7280;line-height:1.5;flex:1}
            .card-footer{display:flex;align-items:center;justify-content:space-between;margin-top:4px}
            .badge{font-size:.72rem;font-weight:600;font-family:monospace;padding:3px 8px;border-radius:4px;white-space:nowrap}
            .run-btn{font-size:.8rem;font-weight:600;padding:7px 16px;border-radius:6px;border:none;cursor:pointer;transition:background .15s,transform .1s;font-family:inherit}
            .run-btn:active{transform:scale(.97)}
            .run-btn:disabled{opacity:.5;cursor:not-allowed}
            .toast{position:fixed;bottom:24px;right:24px;background:#111827;color:#fff;font-size:.82rem;padding:10px 16px;border-radius:8px;font-family:monospace;opacity:0;transform:translateY(8px);transition:opacity .2s,transform .2s;pointer-events:none;max-width:320px;z-index:999}
            .toast.show{opacity:1;transform:translateY(0)}
            .footer{text-align:center;padding:24px;color:#9ca3af;font-size:.82rem;border-top:1px solid #e5e7eb;margin-top:8px}
          </style>
        </head>
        <body>
          <div class="hero">
            <h1>RunWay Demo</h1>
            <p>This app demonstrates RunWay — a background job runner for ASP.NET Core.
               Trigger test jobs below and watch them execute in the dashboard.</p>
            <a class="btn" href="{{basePath}}scheduler">
              <svg width="18" height="18" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24">
                <rect x="3" y="3" width="7" height="7" rx="1"/>
                <rect x="14" y="3" width="7" height="7" rx="1"/>
                <rect x="3" y="14" width="7" height="7" rx="1"/>
                <rect x="14" y="14" width="7" height="7" rx="1"/>
              </svg>
              Open Dashboard
            </a>
            <p class="note">Jobs are picked up and executed by the in-process runner automatically.</p>
          </div>

          <div class="section-title">Test Jobs</div>

          <div class="grid">
            <div class="card">
              <div class="card-header">
                <div class="card-icon" style="background:#eff6ff">
                  <svg width="18" height="18" fill="none" stroke="#2563eb" stroke-width="2" viewBox="0 0 24 24"><path d="m13 2-3 14.5 3-4h5L6 22l3-11.5-3 4H1z"/></svg>
                </div>
                <div>
                  <div class="card-title">Quick Task</div>
                </div>
              </div>
              <div class="card-desc">Simulates a fast background task such as sending a webhook notification or updating a cache entry.</div>
              <div class="card-footer">
                <span class="badge" style="background:#eff6ff;color:#2563eb">~3 seconds</span>
                <button class="run-btn" style="background:#2563eb;color:#fff" onclick="trigger('{{basePath}}jobs/quick', this)">Run</button>
              </div>
            </div>

            <div class="card">
              <div class="card-header">
                <div class="card-icon" style="background:#f0fdf4">
                  <svg width="18" height="18" fill="none" stroke="#16a34a" stroke-width="2" viewBox="0 0 24 24"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/></svg>
                </div>
                <div>
                  <div class="card-title">Report Generation</div>
                </div>
              </div>
              <div class="card-desc">Simulates generating a multi-page report from a database — reads, aggregates and formats rows in batches.</div>
              <div class="card-footer">
                <span class="badge" style="background:#f0fdf4;color:#16a34a">~15 seconds</span>
                <button class="run-btn" style="background:#16a34a;color:#fff" onclick="trigger('{{basePath}}jobs/report', this)">Run</button>
              </div>
            </div>

            <div class="card">
              <div class="card-header">
                <div class="card-icon" style="background:#fffbeb">
                  <svg width="18" height="18" fill="none" stroke="#d97706" stroke-width="2" viewBox="0 0 24 24"><polyline points="16 3 21 3 21 8"/><line x1="4" y1="20" x2="21" y2="3"/><polyline points="21 16 21 21 16 21"/><line x1="15" y1="15" x2="21" y2="21"/></svg>
                </div>
                <div>
                  <div class="card-title">Data Sync</div>
                </div>
              </div>
              <div class="card-desc">Simulates synchronising records between two systems in multiple batches — typical for ETL or cross-service replication.</div>
              <div class="card-footer">
                <span class="badge" style="background:#fffbeb;color:#d97706">~30 seconds</span>
                <button class="run-btn" style="background:#d97706;color:#fff" onclick="trigger('{{basePath}}jobs/sync', this)">Run</button>
              </div>
            </div>

            <div class="card">
              <div class="card-header">
                <div class="card-icon" style="background:#faf5ff">
                  <svg width="18" height="18" fill="none" stroke="#7c3aed" stroke-width="2" viewBox="0 0 24 24"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
                </div>
                <div>
                  <div class="card-title">Large Export</div>
                </div>
              </div>
              <div class="card-desc">Simulates exporting a large dataset to CSV/XLSX — streams records in chunks to avoid memory pressure.</div>
              <div class="card-footer">
                <span class="badge" style="background:#faf5ff;color:#7c3aed">~60 seconds</span>
                <button class="run-btn" style="background:#7c3aed;color:#fff" onclick="trigger('{{basePath}}jobs/export', this)">Run</button>
              </div>
            </div>

            <div class="card">
              <div class="card-header">
                <div class="card-icon" style="background:#fff1f2">
                  <svg width="18" height="18" fill="none" stroke="#e11d48" stroke-width="2" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                </div>
                <div>
                  <div class="card-title">Failing Job</div>
                </div>
              </div>
              <div class="card-desc">Intentionally throws an exception after 2 seconds. Useful for testing error handling, retry policies and failure visibility in the dashboard.</div>
              <div class="card-footer">
                <span class="badge" style="background:#fff1f2;color:#e11d48">always fails</span>
                <button class="run-btn" style="background:#e11d48;color:#fff" onclick="trigger('{{basePath}}jobs/fail', this)">Run</button>
              </div>
            </div>
          </div>

          <div class="footer">RunWay Demo · jobs are processed by the in-process runner</div>

          <div class="toast" id="toast"></div>

          <script>
            function trigger(url, btn) {
              btn.disabled = true;
              fetch(url, { method: 'POST' })
                .then(r => r.ok ? r.text() : Promise.reject(r.status))
                .then(() => showToast('Job queued ✔'))
                .catch(() => showToast('Error — check the console'))
                .finally(() => { setTimeout(() => btn.disabled = false, 1000); });
            }

            function showToast(msg) {
              const t = document.getElementById('toast');
              t.textContent = msg;
              t.classList.add('show');
              setTimeout(() => t.classList.remove('show'), 3000);
            }
          </script>
        </body>
        </html>
        """;
}
