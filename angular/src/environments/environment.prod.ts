import { Environment } from '@abp/ng.core';

const baseUrl = 'https://ntmpf.marhaba-syria.com';

const oAuthConfig = {
  issuer: 'https://ntmpb.marhaba-syria.com/',
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
      url: 'https://ntmpb.marhaba-syria.com',
      rootNamespace: 'SyriaNTMP',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
