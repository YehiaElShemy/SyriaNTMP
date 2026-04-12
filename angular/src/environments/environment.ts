 import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44329/',
  redirectUri: baseUrl,
  clientId: 'SyriaNTMP_App',
  responseType: 'code',
  scope: 'offline_access SyriaNTMP',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'SyriaNTMP',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44329',
      rootNamespace: 'SyriaNTMP',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
