import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';
//import { config } from './app/app.config.server';
import { provideServerRendering } from '@angular/ssr';
import { provideRouter } from '@angular/router';
import { provideHttpClient,withFetch } from '@angular/common/http';
import { routes } from './app/app.routes';
import 'zone.js';
export default function bootstrap(){
return bootstrapApplication(App,{
    providers:[
        provideServerRendering(),
        provideRouter(routes),
        provideHttpClient(withFetch()),
    ],
});
}
