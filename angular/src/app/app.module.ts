import { CoreModule, provideAbpCore, withOptions } from '@abp/ng.core';
import { provideAbpOAuth } from '@abp/ng.oauth';
import { provideSettingManagementConfig } from '@abp/ng.setting-management/config';
import { provideFeatureManagementConfig } from '@abp/ng.feature-management';
import { ThemeSharedModule, provideAbpThemeShared } from '@abp/ng.theme.shared';
import { provideIdentityConfig } from '@abp/ng.identity/config';
import { provideAccountConfig } from '@abp/ng.account/config';
import { registerLocale } from '@abp/ng.core/locale';
import { ThemeBasicModule, provideThemeBasicConfig } from '@abp/ng.theme.basic';

import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { providePrimeNG } from 'primeng/config';
import Lara from '@primeuix/themes/lara';
import { environment } from '../environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { APP_ROUTE_PROVIDER } from './route.provider';
import { SharedModule } from './shared/shared.module';

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    SharedModule,
    ThemeSharedModule,
    CoreModule,
    ThemeBasicModule,
  ],
  providers: [
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
        preset: Lara
      }
    })
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}