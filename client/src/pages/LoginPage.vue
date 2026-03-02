<template>
  <div class="auth-bg">
    <div class="bg-grid"></div>
    <div class="glow glow-1"></div>
    <div class="glow glow-2"></div>

    <div class="auth-card">
      <div class="icon-wrap">
        <img src="/icon.png" class="site-icon" alt="logo" />
      </div>

      <h1 class="title">Scheme App</h1>
      <p class="subtitle">{{ isLogin ? 'Войдите в аккаунт' : 'Создайте аккаунт' }}</p>

      <div class="tab-switch">
        <button :class="['tab', { active: isLogin }]" @click="isLogin = true">Вход</button>
        <button :class="['tab', { active: !isLogin }]" @click="isLogin = false">Регистрация</button>
      </div>

      <div class="form">
        <div class="field">
          <label>Email</label>
          <input
            v-model="email"
            type="email"
            placeholder="you@example.com"
            @keydown.enter="submit"
          />
        </div>

        <div class="field">
          <label>Пароль</label>
          <div class="input-wrap">
            <input
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="••••••••"
              @keydown.enter="submit"
            />
            <button class="eye-btn" @click="showPassword = !showPassword" tabindex="-1">
              {{ showPassword ? '🙈' : '👁️' }}
            </button>
          </div>
        </div>

        <div v-if="error" class="error-msg">{{ error }}</div>

        <button class="btn-submit" @click="submit" :disabled="loading">
          <span v-if="loading" class="spinner"></span>
          <span v-else>{{ isLogin ? 'Войти' : 'Зарегистрироваться' }}</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const router = useRouter()

const isLogin = ref(true)
const email = ref('')
const password = ref('')
const showPassword = ref(false)
const loading = ref(false)
const error = ref('')

const API = 'http://localhost:5204/api/auth'

const submit = async () => {
  error.value = ''

  if (!email.value || !password.value) {
    error.value = 'Заполните все поля'
    return
  }

  loading.value = true
  try {
    const endpoint = isLogin.value ? `${API}/login` : `${API}/register`
    const { data } = await axios.post(endpoint, {
      email: email.value,
      password: password.value
    })
    localStorage.setItem('token', data.token)
    router.push('/')
  } catch (err) {
    error.value = err.response?.data || 'Что-то пошло не так'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Syne:wght@400;600;700&family=DM+Sans:wght@300;400;500&display=swap');

* { box-sizing: border-box; margin: 0; padding: 0; }

.auth-bg {
  min-height: 100vh;
  background: #090b10;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
  font-family: 'DM Sans', sans-serif;
}

.bg-grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(to right, rgba(45, 52, 70, 0.4) 1px, transparent 1px),
    linear-gradient(to bottom, rgba(45, 52, 70, 0.4) 1px, transparent 1px);
  background-size: 80px 80px;
  pointer-events: none;
}

.glow {
  position: absolute;
  border-radius: 50%;
  filter: blur(120px);
  pointer-events: none;
  opacity: 0.25;
  animation: pulse 8s ease-in-out infinite alternate;
}
.glow-1 {
  width: 500px; height: 500px;
  background: #3b82f6;
  top: -150px; left: -100px;
}
.glow-2 {
  width: 400px; height: 400px;
  background: #6366f1;
  bottom: -100px; right: -80px;
  animation-delay: -4s;
}

@keyframes pulse {
  from { opacity: 0.15; transform: scale(1); }
  to   { opacity: 0.3;  transform: scale(1.1); }
}

.auth-card {
  position: relative;
  z-index: 10;
  width: 400px;
  background: rgba(17, 20, 31, 0.85);
  backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: 16px;
  padding: 40px 36px 36px;
  box-shadow:
    0 0 0 1px rgba(255,255,255,0.03),
    0 40px 80px rgba(0, 0, 0, 0.6),
    0 0 60px rgba(59, 130, 246, 0.05);
  animation: cardIn 0.5s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes cardIn {
  from { opacity: 0; transform: translateY(24px) scale(0.97); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}

.icon-wrap {
  display: flex;
  justify-content: center;
  margin-bottom: 16px;
}
.site-icon {
  width: 52px;
  height: 52px;
  border-radius: 12px;
  border: 1px solid rgba(255,255,255,0.1);
  padding: 6px;
  background: rgba(255,255,255,0.04);
}

.title {
  font-family: 'Syne', sans-serif;
  font-size: 22px;
  font-weight: 700;
  color: #f1f5f9;
  text-align: center;
  letter-spacing: -0.3px;
}
.subtitle {
  color: #64748b;
  font-size: 13px;
  text-align: center;
  margin-top: 4px;
  margin-bottom: 24px;
}

.tab-switch {
  display: flex;
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.07);
  border-radius: 8px;
  padding: 3px;
  margin-bottom: 24px;
  gap: 3px;
}
.tab {
  flex: 1;
  padding: 8px;
  border: none;
  border-radius: 6px;
  background: transparent;
  color: #64748b;
  font-size: 13px;
  font-family: 'DM Sans', sans-serif;
  cursor: pointer;
  transition: all 0.2s;
  font-weight: 500;
}
.tab.active {
  background: rgba(59, 130, 246, 0.15);
  color: #93c5fd;
  border: 1px solid rgba(59, 130, 246, 0.25);
}

.form { display: flex; flex-direction: column; gap: 16px; }

.field { display: flex; flex-direction: column; gap: 6px; }
.field label {
  font-size: 12px;
  color: #94a3b8;
  font-weight: 500;
  letter-spacing: 0.3px;
}

.field input, .input-wrap input {
  width: 100%;
  background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.09);
  border-radius: 8px;
  padding: 10px 14px;
  color: #f1f5f9;
  font-size: 14px;
  font-family: 'DM Sans', sans-serif;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}
.field input:focus, .input-wrap input:focus {
  border-color: rgba(59, 130, 246, 0.5);
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}
.field input::placeholder, .input-wrap input::placeholder {
  color: #334155;
}

.input-wrap {
  position: relative;
}
.input-wrap input {
  padding-right: 40px;
}
.eye-btn {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  cursor: pointer;
  font-size: 14px;
  opacity: 0.5;
  transition: opacity 0.2s;
  padding: 0;
  line-height: 1;
}
.eye-btn:hover { opacity: 1; }

.error-msg {
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.25);
  border-radius: 8px;
  color: #fca5a5;
  font-size: 13px;
  padding: 10px 14px;
  animation: shake 0.3s ease;
}
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-6px); }
  75% { transform: translateX(6px); }
}

.btn-submit {
  margin-top: 4px;
  width: 100%;
  padding: 12px;
  background: linear-gradient(135deg, #3b82f6, #6366f1);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  font-family: 'Syne', sans-serif;
  cursor: pointer;
  letter-spacing: 0.3px;
  transition: opacity 0.2s, transform 0.15s, box-shadow 0.2s;
  box-shadow: 0 4px 20px rgba(59, 130, 246, 0.25);
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 44px;
}
.btn-submit:hover:not(:disabled) {
  opacity: 0.9;
  transform: translateY(-1px);
  box-shadow: 0 8px 28px rgba(59, 130, 246, 0.35);
}
.btn-submit:active:not(:disabled) {
  transform: translateY(0);
}
.btn-submit:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>