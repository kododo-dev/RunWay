// `npm run dev`: the dashboard inside WayDevHost, with the API of the demo app through Vite's proxy.
import { createRoot } from 'react-dom/client';
import { WayDevHost } from '@kododo-dev/way-shell';
import way from './way';

createRoot(document.getElementById('root')!).render(
  <WayDevHost way={way} id="scheduler" name="Scheduler" apiBase="/scheduler/api" />,
);
