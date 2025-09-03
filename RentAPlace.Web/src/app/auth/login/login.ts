import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { Auth } from '../../services/auth';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [FormsModule, CommonModule, RouterModule],
    templateUrl: './login.html',
    styleUrls: ['./login.css']
})
export class Login {
    email = '';
    password = '';
    error = '';
    success = ''; // <-- added success property

    constructor(
        private auth: Auth,
        private router: Router,
        @Inject(PLATFORM_ID) private platformId: any
    ) { }

    onLogin(form: NgForm) {
        if (form.invalid) {
            Object.values(form.controls).forEach(c => c.markAsTouched());
            return;
        }

        this.auth.login(this.email, this.password).subscribe({
            next: res => {
                if (isPlatformBrowser(this.platformId)) {
                    sessionStorage.setItem('token', res.accessToken);
                    sessionStorage.setItem('user', JSON.stringify(res.user));
                }
                alert('Login successful!');
                this.router.navigate(['/dashboard']);
            },
            error: () => {
                alert('Invalid email or password');
            }
        });
    }

}

