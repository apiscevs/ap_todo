import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface TodoItem {
  id: string;
  title: string;
  isCompleted: boolean;
  createdAt: string;
  completedAt?: string | null;
}

export interface TodoCreateRequest {
  title: string;
  isCompleted: boolean;
}

export interface TodoUpdateRequest {
  title?: string | null;
  isCompleted?: boolean | null;
}

@Injectable({
  providedIn: 'root'
})
export class TodoService {
  private readonly baseUrl = `${environment.apiUrl}/api/todos`;

  constructor(private http: HttpClient) {}

  getTodos(): Observable<TodoItem[]> {
    return this.http.get<TodoItem[]>(this.baseUrl);
  }

  getTodo(id: string): Observable<TodoItem> {
    return this.http.get<TodoItem>(`${this.baseUrl}/${id}`);
  }

  createTodo(title: string): Observable<TodoItem> {
    const payload: TodoCreateRequest = {
      title,
      isCompleted: false
    };
    return this.http.post<TodoItem>(this.baseUrl, payload);
  }

  updateTodo(id: string, payload: TodoUpdateRequest): Observable<TodoItem> {
    return this.http.put<TodoItem>(`${this.baseUrl}/${id}`, payload);
  }

  deleteTodo(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  clearCompleted(): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/completed`);
  }
}
