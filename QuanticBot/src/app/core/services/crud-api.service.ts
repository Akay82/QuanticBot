import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { EntityBase, PagedResponse, QueryParams } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class CrudApiService {
  private readonly http = inject(HttpClient);

  getPage<T>(url: string, query: QueryParams = {}): Observable<PagedResponse<T>> {
    return this.http.get<PagedResponse<T>>(url, { params: this.toHttpParams(query) });
  }

  getById<T>(url: string): Observable<T> {
    return this.http.get<T>(url);
  }

  create<T>(url: string, entity: Omit<T, 'id'>): Observable<T> {
    return this.http.post<T>(url, entity);
  }

  update<T extends EntityBase>(url: string, entity: T): Observable<T> {
    return this.http.put<T>(url, entity);
  }

  delete(url: string): Observable<void> {
    return this.http.delete<void>(url);
  }

  private toHttpParams(query: QueryParams): HttpParams {
    return Object.entries(query).reduce((params, [key, value]) => {
      return value === undefined || value === null ? params : params.set(key, String(value));
    }, new HttpParams());
  }
}
