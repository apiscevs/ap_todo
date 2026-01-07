import { Component, OnInit } from '@angular/core';
import { TodoItem, TodoService } from './todo.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Studio Todo';
  newTask = '';
  filter: 'all' | 'active' | 'done' = 'all';
  todos: TodoItem[] = [];
  isLoading = false;
  errorMessage = '';

  get visibleTodos() {
    if (this.filter === 'active') {
      return this.todos.filter((todo) => !todo.isCompleted);
    }
    if (this.filter === 'done') {
      return this.todos.filter((todo) => todo.isCompleted);
    }
    return this.todos;
  }

  get remainingCount() {
    return this.todos.filter((todo) => !todo.isCompleted).length;
  }

  constructor(private todoService: TodoService) {}

  ngOnInit() {
    this.loadTodos();
  }

  loadTodos() {
    this.isLoading = true;
    this.errorMessage = '';
    this.todoService.getTodos().subscribe({
      next: (todos) => {
        this.todos = todos;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load todos. Check that the backend is running.';
        this.isLoading = false;
      }
    });
  }

  addTodo() {
    const title = this.newTask.trim();
    if (!title) {
      return;
    }
    this.todoService.createTodo(title).subscribe({
      next: (todo) => {
        this.todos = [todo, ...this.todos];
        this.newTask = '';
      },
      error: () => {
        this.errorMessage = 'Unable to add todo. Try again.';
      }
    });
  }

  toggleTodo(todoId: string) {
    const todo = this.todos.find((item) => item.id === todoId);
    if (!todo) {
      return;
    }
    this.todoService
      .updateTodo(todo.id, { isCompleted: !todo.isCompleted })
      .subscribe({
        next: (updated) => {
          this.todos = this.todos.map((item) => (item.id === updated.id ? updated : item));
        },
        error: () => {
          this.errorMessage = 'Unable to update todo. Try again.';
        }
      });
  }

  removeTodo(todoId: string) {
    this.todoService.deleteTodo(todoId).subscribe({
      next: () => {
        this.todos = this.todos.filter((todo) => todo.id !== todoId);
      },
      error: () => {
        this.errorMessage = 'Unable to remove todo. Try again.';
      }
    });
  }

  clearCompleted() {
    this.todoService.clearCompleted().subscribe({
      next: () => {
        this.todos = this.todos.filter((todo) => !todo.isCompleted);
      },
      error: () => {
        this.errorMessage = 'Unable to clear completed todos. Try again.';
      }
    });
  }
}
