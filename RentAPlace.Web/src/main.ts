import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { LocationStrategy, HashLocationStrategy } from '@angular/common';

import { App } from './app/app';
import { routes } from './app/app.routes';
import 'zone.js';

bootstrapApplication(App, {
  providers: [
    provideRouter(routes),
    provideHttpClient(),
    // use hash routing to avoid server 404s on refresh during dev
    { provide: LocationStrategy, useClass: HashLocationStrategy }
  ]
}).catch(err => console.error(err));
