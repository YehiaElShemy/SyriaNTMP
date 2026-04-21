import { Environment } from '@abp/ng.core';

const baseUrl = 'https://ntmpf.marhaba-syria.sy';

const oAuthConfig = {
  issuer: 'https://ntmpb.marhaba-syria.sy/',
  redirectUri: baseUrl,
  clientId: 'SyriaNTMP_App',
  responseType: 'code',
  scope: 'offline_access SyriaNTMP',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'SyriaNTMP',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://ntmpb.marhaba-syria.sy',
      rootNamespace: 'SyriaNTMP',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
