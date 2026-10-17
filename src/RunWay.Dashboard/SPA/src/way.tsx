import { defineWay } from '@kododo-dev/way-shell';
import App from './App';
import { runWayTheme } from './theme';
import './fonts.css';

// The module Way.Shell loads (built by `npm run build` into dist/way.js and dist/way.css).
export default defineWay({ App, theme: runWayTheme });
