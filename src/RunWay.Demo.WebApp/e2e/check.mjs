// Browser scenarios of the dashboard in Way.Shell, against the running demo (Production, AllowAnonymous):
//   node check.mjs http://localhost:5092 [chrome|msedge|firefox|webkit]
// Schedules a few jobs through the demo's own endpoints first. Needs the playwright package
// (npm i --no-save playwright); firefox and webkit need `npx playwright install`.
import { chromium, firefox, webkit } from 'playwright';

const base = process.argv[2].replace(/\/$/, '');
const which = process.argv[3] ?? 'chrome';
const browser = which === 'firefox' ? await firefox.launch() : which === 'webkit' ? await webkit.launch() : await chromium.launch({ channel: which });
const results = [];
const record = (name, ok, detail = '') => results.push({ name, ok, detail });
const visible = (l, timeout = 10000) => l.first().waitFor({ timeout }).then(() => true, () => false);
const hashIs = (page, hash) => page.waitForURL((u) => u.hash === hash, { timeout: 10000 }).then(() => true, () => false);

for (const job of ['quick', 'fail']) await fetch(`${base}/jobs/${job}`, { method: 'POST' });

async function open(options = {}) {
  const context = await browser.newContext({ locale: 'en-US', colorScheme: 'light', ...options });
  const page = await context.newPage();
  const errors = [];
  page.on('pageerror', (e) => errors.push(e.message));
  page.on('console', (m) => { if (m.type() === 'error' && !m.text().startsWith('Failed to load resource')) errors.push(m.text()); });
  page.on('response', (r) => { if (r.status() >= 400 && !r.url().endsWith('/favicon.ico')) errors.push(`${r.status()} ${r.url()}`); });
  return { context, page, errors };
}

{
  const { context, page, errors } = await open();
  await page.goto(`${base}/scheduler/`);
  record('dashboard opens in the shell', await visible(page.getByRole('heading', { name: 'Dashboard' })));
  record('title from the Way', (await page.title()) === 'Scheduler', await page.title());
  record('status tiles', await visible(page.getByText('PENDING')));
  const menu = page.getByTestId('way-menu');
  record("dashboard's menu in the shell menu", await visible(menu.getByText('Recurrences')));

  // The failing job reaches Failed after its retries; Pending, Running or Retrying hold it until then.
  await menu.getByText('Failed').click();
  record('jobs by status from the menu', await hashIs(page, '#/jobs/category/failed'), page.url());
  const failedRow = page.locator('.MuiDataGrid-row').filter({ hasText: 'FailingJob' });
  const hasFailed = await visible(failedRow, 60000);
  record('data grid with a failed job', hasFailed);

  if (hasFailed) {
    await failedRow.first().click();
    record('job page from a grid row', (await visible(page.getByRole('heading', { name: /^Job #/ }))) && /#\/jobs\/[^/]+$/.test(new URL(page.url()).hash), page.url());
    const requeue = page.getByRole('button', { name: 'Requeue' });
    if (await visible(requeue, 5000)) {
      const [response] = await Promise.all([
        page.waitForResponse((r) => r.url().endsWith('/scheduler/api/RequeueJob')),
        requeue.click(),
      ]);
      record('requeue through the shell API client', response.ok(), `${response.status()}`);
    } else {
      record('requeue through the shell API client', false, 'no Requeue button');
    }
  }

  await menu.getByText('Recurrences').click();
  record('menu from a nested page (paths from the Way root)', await hashIs(page, '#/recurrences'), page.url());
  await visible(page.locator('.MuiDataGrid-row'));
  await page.locator('.MuiDataGrid-row').first().click();
  record('recurrence page', await visible(page.getByRole('heading', { name: /^Recurrence #/ })));
  await menu.getByText('Runners').click();
  record('runners from the menu', await hashIs(page, '#/runners'), page.url());

  await page.goto(`${base}/scheduler/#/nope`);
  record('unknown page goes to the Way root', await hashIs(page, '#/'), page.url());

  await page.getByRole('button', { name: 'Language' }).click();
  await page.getByRole('menuitem', { name: 'Polski' }).click();
  record('shell language switch', await visible(page.getByRole('button', { name: 'Ciemny motyw' })));
  await page.getByRole('button', { name: 'Ciemny motyw' }).click();
  const header = await page.locator('main header').first().evaluate((e) => getComputedStyle(e).backgroundColor);
  record("dashboard's dark paper in its area", header === 'rgb(36, 36, 36)', header);
  const font = await page.locator('main h1').first().evaluate((e) => getComputedStyle(e).fontFamily);
  record("dashboard's monospace theme in its area", /IBM Plex Mono/.test(font), font);
  record('no errors', errors.length === 0, errors.join(' | '));
  await context.close();
}

{
  const { context, page } = await open({ viewport: { width: 390, height: 800 } });
  await page.goto(`${base}/scheduler/#/jobs/category/pending`);
  await visible(page.getByRole('heading', { name: 'Pending jobs' }));
  await page.getByRole('button', { name: 'Open menu' }).click();
  await page.getByTestId('way-menu').getByText('Runners').click();
  const closed = await page.getByTestId('way-menu').waitFor({ state: 'hidden', timeout: 5000 }).then(() => true, () => false);
  record('phone: an item of the menu closes it', closed && (await hashIs(page, '#/runners')), page.url());
  await context.close();
}

await browser.close();
const failed = results.filter((r) => !r.ok);
for (const r of results) console.log(`${r.ok ? 'ok  ' : 'FAIL'} ${r.name}${r.detail ? ` — ${r.detail}` : ''}`);
console.log(`\n${which} ${base}: ${results.length - failed.length}/${results.length} ok`);
process.exit(failed.length ? 1 : 0);
