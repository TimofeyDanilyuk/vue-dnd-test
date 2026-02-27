<template>
  <div class="app-container">
    <aside class="sidebar">
      <div class="sidebar-header">
        <h2 class="sidebar-title">Палитра</h2>
      </div>

      <VueDraggable
        v-model="palette"
        :group="{ name: 'scheme', pull: 'clone', put: false }"
        :sort="false"
        :clone="cloneItem"
        item-key="id"
        class="palette-list"
        tag="div"
      >
        <div v-for="it in palette" :key="it.id" class="palette-item">
          <img :src="it.src" class="thumb" />
        </div>
      </VueDraggable>
    </aside>

    <main class="main-content">
      <header class="topbar">
        <div class="topbar-info">
          <h1 class="topbar-title">Полотно</h1>
          <div class="count-badge">Всего: {{ nodes.length }}</div>
        </div>
        <button class="btn-clear" @click="nodes = []">Очистить</button>
      </header>

      <div ref="canvasEl" class="canvas-wrapper">
        <VueDraggable
          v-model="nodes"
          :group="{ name: 'scheme', put: true }"
          :sort="false"
          item-key="id"
          @add="handleDrop"
          class="canvas-area"
          tag="div"
        >
          <div
            v-for="n in nodes"
            :key="n.id"
            class="node-element"
            :style="{ left: n.x + 'px', top: n.y + 'px' }"
          >
            <img :src="n.src" class="thumb-canvas" draggable="false" />
          </div>
        </VueDraggable>

        <div v-if="nodes.length === 0" class="empty-msg">
           Перетащите элементы из палитры
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref } from "vue";
import { VueDraggable } from "vue-draggable-plus";

const canvasEl = ref(null);
const nodes = ref([]);
const palette = ref([]);

// берем картинки из папки
const files = import.meta.glob("@/assets/palette/*.{png,jpg,jpeg,webp,svg,gif}", {
  eager: true,
  import: "default",
});

// заполняем список палитры
palette.value = Object.entries(files).map(([path, url]) => ({
  id: path,
  src: url
}));

// создаем копию объекта для холста
function cloneItem(item) {
  return {
    ...item,
    id: crypto.randomUUID(),
    x: 0,
    y: 0
  };
}

const GRID_STEP = 100; // Шаг сетки в пикселях

// считаем координаты при дропе с привязкой к сетке
function handleDrop(evt) {
  const e = evt.originalEvent;
  const box = canvasEl.value?.getBoundingClientRect();

  if (!e || !box) return;

  const clientX = e.touches?.[0]?.clientX ?? e.clientX;
  const clientY = e.touches?.[0]?.clientY ?? e.clientY;

  const index = evt.newIndex;
  
  // ставим координаты в массив с округлением до шага сетки
  requestAnimationFrame(() => {
    const item = nodes.value[index];
    if (item) {
      const rawX = clientX - box.left - 40; 
      const rawY = clientY - box.top - 40;

      item.x = Math.round(rawX / GRID_STEP) * GRID_STEP;
      item.y = Math.round(rawY / GRID_STEP) * GRID_STEP;
    }
  });
}
</script>

<style>
html, body, #app {
  margin: 0; padding: 0;
  height: 100vh; width: 100vw;
  overflow: hidden;
  font-family: sans-serif;
  background: #090b10;
  color: #e0e6ed;
}
</style>

<style scoped>
.app-container {
  display: grid;
  grid-template-columns: 200px 1fr;
  height: 100vh;
}

.sidebar {
  background: #11141f;
  border-right: 1px solid #2d3446;
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  padding: 15px;
  border-bottom: 1px solid #2d3446;
}

.sidebar-title {
  margin: 0; font-size: 16px;
}

.palette-list {
  flex: 1; overflow-y: auto;
  padding: 10px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
  align-content: start;
}

.palette-item {
  background: #1a1f2e;
  border: 1px solid #2d3446;
  border-radius: 6px;
  padding: 8px;
  cursor: grab;
  display: flex;
  justify-content: center;
  align-items: center;
  aspect-ratio: 1 / 1;
}

.palette-item:hover {
  border-color: #3b82f6;
  background: #252b3d;
}

.thumb {
  max-width: 100%;
  max-height: 100%;
  width: auto;
  height: auto; 
  object-fit: contain; 
}

.main-content {
  display: flex;
  flex-direction: column;
  background: #090b10;
}

.topbar {
  height: 50px;
  background: #11141f;
  border-bottom: 1px solid #2d3446;
  padding: 0 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.topbar-info { display: flex; align-items: center; gap: 10px; }
.topbar-title { font-size: 16px; margin: 0; }

.count-badge {
  font-size: 11px;
  background: rgba(59, 130, 246, 0.2);
  color: #3b82f6;
  padding: 2px 8px;
  border-radius: 10px;
}

.btn-clear {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
  border: 1px solid #ef4444;
  padding: 5px 12px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-clear:hover { background: #ef4444; color: white; }

.canvas-wrapper {
  flex: 1;
  position: relative;
  background-image: radial-gradient(#3d475c 1px, transparent 1px);
  background-size: 20px 20px;
}

.canvas-area { width: 100%; height: 100%; position: relative; }

.node-element {
  position: absolute;
  background: #1a1f2e;
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 8px;
  padding: 4px;
  box-shadow: 0 4px 10px rgba(0,0,0,0.5);
  display: flex;
  justify-content: center;
  align-items: center;
}

.thumb-canvas {
  width: 80px; 
  height: 80px;
  display: block;
  object-fit: contain;
}

.node-img {
  display: none;
}

.empty-msg {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
  pointer-events: none;
}
</style>