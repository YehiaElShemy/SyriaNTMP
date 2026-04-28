import { CoreModule, provideAbpCore, withOptions } from '@abp/ng.core';
import { provideAbpOAuth } from '@abp/ng.oauth';
import { provideSettingManagementConfig } from '@abp/ng.setting-management/config';
import { provideFeatureManagementConfig } from '@abp/ng.feature-management';
import { ThemeSharedModule, provideAbpThemeShared, HTTP_ERROR_HANDLER } from '@abp/ng.theme.shared';
import { provideIdentityConfig } from '@abp/ng.identity/config';
import { provideAccountConfig } from '@abp/ng.account/config';
import { registerLocale } from '@abp/ng.core/locale';
import { ThemeBasicModule, provideThemeBasicConfig } from '@abp/ng.theme.basic';

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { provideTranslateHttpLoader } from '@ngx-translate/http-loader';

import { providePrimeNG } from 'primeng/config';
import Lara from '@primeuix/themes/lara';
import { environment } from '../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { NotfoundComponent } from './pages/notfound/notfound.component';
import { AppComponent } from './app.component';
import { APP_ROUTE_PROVIDER } from './route.provider';
import { SharedModule } from './shared/shared.module';
import { SErrorHandlerService } from './shared/services/s-error-handler';

@NgModule({
  declarations: [AppComponent, NotfoundComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    SharedModule,
    ThemeSharedModule,
    CoreModule,
    ThemeBasicModule,
    TranslateModule.forRoot(),
  ],
  providers: [
    provideTranslateHttpLoader({ prefix: './assets/i18n/', suffix: '.json' }),
    APP_ROUTE_PROVIDER,

    provideAbpCore(
      withOptions({
        environment,
        registerLocaleFn: registerLocale(),
      })
    ),
    provideAbpOAuth(),
    provideIdentityConfig(),
    provideSettingManagementConfig(),
    provideFeatureManagementConfig(),
    provideAccountConfig(),
    provideAbpThemeShared(),
    provideThemeBasicConfig(),

    // ✅ ADD THIS (PrimeNG)
    providePrimeNG({
      theme: {
        preset: Lara,
        options: {
          darkModeSelector: '.app-dark'
        }
      }
    }),
    {
      provide: HTTP_ERROR_HANDLER,
      useClass: SErrorHandlerService,
      multi: true, // Multi is required as there can be multiple handlers
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule { }