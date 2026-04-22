import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { CustomHttpErrorHandlerService } from '@abp/ng.theme.shared';

@Injectable({ providedIn: 'root' })
export class SErrorHandlerService implements CustomHttpErrorHandlerService {
  execute(): void {
    console.error('Custom error logic for:');
  }
  // Higher priority runs before default ABP handlers
  priority = 100;

  canHandle(error: HttpErrorResponse): boolean {
    console.error('Custom error logic for:', error);
    // Check for specific status codes or error patterns
    return error.status === 418;
  }

}
