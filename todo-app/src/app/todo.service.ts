import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
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
  isCompleted?: boolean | null;
}

export interface TodoUpdateRequest {
  title?: string | null;
  isCompleted?: boolean | null;
}

interface GraphQLError {
  message: string;
}

interface GraphQLResponse<T> {
  data?: T;
  errors?: GraphQLError[];
}

@Injectable({
  providedIn: 'root'
})
export class TodoService {
  private readonly graphQlUrl = `${environment.apiUrl}/graphql`;

  constructor(private http: HttpClient) {}

  getTodos(): Observable<TodoItem[]> {
    const query = `
      query GetTodos {
        todos {
          id
          title
          isCompleted
          createdAt
          completedAt
        }
      }
    `;

    return this.executeGraphQL<{ todos: TodoItem[] }>(query).pipe(map((data) => data.todos));
  }

  getTodo(id: string): Observable<TodoItem> {
    const query = `
      query GetTodo($id: UUID!) {
        todo(id: $id) {
          id
          title
          isCompleted
          createdAt
          completedAt
        }
      }
    `;

    return this.executeGraphQL<{ todo: TodoItem | null }>(query, { id }).pipe(
      map((data) => {
        if (!data.todo) {
          throw new Error('Todo not found.');
        }
        return data.todo;
      })
    );
  }

  createTodo(title: string): Observable<TodoItem> {
    const payload: TodoCreateRequest = {
      title
    };
    const mutation = `
      mutation CreateTodo($input: TodoCreateInput!) {
        createTodo(input: $input) {
          id
          title
          isCompleted
          createdAt
          completedAt
        }
      }
    `;

    return this.executeGraphQL<{ createTodo: TodoItem }>(mutation, { input: payload }).pipe(
      map((data) => data.createTodo)
    );
  }

  updateTodo(id: string, payload: TodoUpdateRequest): Observable<TodoItem> {
    const mutation = `
      mutation UpdateTodo($id: UUID!, $input: TodoUpdateInput!) {
        updateTodo(id: $id, input: $input) {
          id
          title
          isCompleted
          createdAt
          completedAt
        }
      }
    `;

    return this.executeGraphQL<{ updateTodo: TodoItem }>(mutation, { id, input: payload }).pipe(
      map((data) => data.updateTodo)
    );
  }

  deleteTodo(id: string): Observable<void> {
    const mutation = `
      mutation DeleteTodo($id: UUID!) {
        deleteTodo(id: $id)
      }
    `;

    return this.executeGraphQL<{ deleteTodo: boolean }>(mutation, { id }).pipe(map(() => undefined));
  }

  clearCompleted(): Observable<void> {
    const mutation = `
      mutation DeleteCompletedTodos {
        deleteCompletedTodos
      }
    `;

    return this.executeGraphQL<{ deleteCompletedTodos: number }>(mutation).pipe(map(() => undefined));
  }

  private executeGraphQL<T>(query: string, variables?: Record<string, unknown>): Observable<T> {
    return this.http
      .post<GraphQLResponse<T>>(this.graphQlUrl, {
        query,
        variables
      })
      .pipe(
        map((response) => {
          if (response.errors?.length) {
            const message = response.errors.map((error) => error.message).join('\n');
            throw new Error(message);
          }
          if (!response.data) {
            throw new Error('GraphQL response missing data.');
          }
          return response.data;
        })
      );
  }
}
