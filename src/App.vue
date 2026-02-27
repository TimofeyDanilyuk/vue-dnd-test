<template>
  <div class="app-container">
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="sidebar-title-row">
          <img src="/icon.png" class="sidebar-icon" alt="" />
          <h2 class="sidebar-title">Палитра</h2>
        </div>
      </div>

      <VueDraggable
        v-model="palette"
        :group="{ name: 'scheme', pull: 'clone', put: false }"
        :sort="false"
        :clone="cloneItem"
        item-key="id"
        :force-fallback="true"
        fallback-class="sortable-fallback"
        class="palette-list"
      >
        <div v-for="it in palette" :key="it.id" class="palette-item">
          <img :src="it.src" class="thumb" />
          <div class="size-tag">{{ it.w }}x{{ it.h }}</div>
        </div>
      </VueDraggable>
    </aside>

    <main class="main-content">
      <header class="topbar">
        <div class="topbar-info">
          <h1 class="topbar-title">Полотно</h1>
          <div class="count-badge">Всего: {{ nodes.length }}</div>
        </div>
        <div class="topbar-actions">
          <button class="btn-download" @click="downloadScheme">💾 Сохранить PNG</button>
          <button class="btn-clear" @click="nodes = []">🗑️ Очистить</button>
        </div>
      </header>

      <div ref="canvasEl" class="canvas-wrapper">
        <VueDraggable
          v-model="nodes"
          :group="{ name: 'scheme', put: true }"
          :sort="false"
          item-key="id"
          :force-fallback="true"
          @add="handleDrop"
          class="canvas-area"
        >
          <div
            v-for="n in nodes"
            :key="n.id"
            class="node-element"
            :style="{ 
              left: n.x + 'px', 
              top: n.y + 'px',
              width: n.w + 'px',
              height: n.h + 'px'
            }"
            @click="deleteNode(n.id)" 
          >
            <img :src="n.src" class="thumb-canvas" draggable="false" />
            <div class="delete-icon">×</div>
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
import { ref, onMounted } from "vue";
import { VueDraggable } from "vue-draggable-plus";
import { toPng } from 'html-to-image';

const GRID_STEP = 80;
const canvasEl = ref(null);
const nodes = ref([]);

// 1. Сначала импортируем файлы
const files = import.meta.glob("@/assets/palette/*.{png,jpg,jpeg,webp,svg,gif}", {
  eager: true,
  import: "default",
});

// 2. Сразу создаем палитру, чтобы Draggable увидел элементы при старте
const palette = ref(Object.entries(files).map(([path, url]) => ({
  id: path,
  src: url,
  w: 80,
  h: 80
})));

const getImageSize = (url) => {
  return new Promise((resolve) => {
    const img = new Image();
    img.onload = () => resolve({ w: img.naturalWidth, h: img.naturalHeight });
    img.onerror = () => resolve({ w: GRID_STEP, h: GRID_STEP });
    img.src = url;
  });
};

// 3. Догружаем реальные размеры
onMounted(async () => {
  for (const item of palette.value) {
    const size = await getImageSize(item.src);
    item.w = size.w;
    item.h = size.h;
  }
});

function cloneItem(item) {
  return {
    ...item,
    id: crypto.randomUUID(),
    x: 0,
    y: 0
  };
}

function handleDrop(evt) {
  const e = evt.originalEvent;
  const box = canvasEl.value?.getBoundingClientRect();
  if (!e || !box) return;

  const clientX = (e.touches ? e.touches[0].clientX : e.clientX);
  const clientY = (e.touches ? e.touches[0].clientY : e.clientY);
  
  const index = evt.newIndex;
  
  // Даем Vue время добавить элемент в массив nodes
  setTimeout(() => {
    const item = nodes.value[index];
    if (item) {
      const rawX = clientX - box.left - (item.w / 2);
      const rawY = clientY - box.top - (item.h / 2);

      item.x = Math.round(rawX / GRID_STEP) * GRID_STEP;
      item.y = Math.round(rawY / GRID_STEP) * GRID_STEP;
    }
  }, 0);
}

const downloadScheme = async () => {
  if (!canvasEl.value) return;
  try {
    const dataUrl = await toPng(canvasEl.value, { 
      cacheBust: true,
      backgroundColor: '#090b10' 
    });
    const link = document.createElement('a');
    link.download = `схема-${Date.now()}.png`;
    link.href = dataUrl;
    link.click();
  } catch (err) {
    console.error('Ошибка PNG:', err);
  }
};

const deleteNode = (id) => {
  nodes.value = nodes.value.filter(n => n.id !== id);
};
</script>

<style>
html, body, #app {
  margin: 0; padding: 0;
  height: 100vh; width: 100vw;
  overflow: hidden;
  font-family: sans-serif;
  background: #18181b; /* Мягкий темный */
  color: #d4d4d8;
}
</style>

<style scoped>
.app-container {
  display: grid;
  grid-template-columns: 180px 1fr;
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

.sidebar-title { margin: 0; font-size: 16px; }

.palette-list {
  flex: 1; overflow-y: auto;
  padding: 10px;
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  align-content: start;
}

.palette-item {
  background: #1a1f2e;
  border: 1px solid #2d3446;
  border-radius: 6px;
  padding: 8px;
  cursor: grab;
  position: relative;
  aspect-ratio: 1 / 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.size-tag {
  position: absolute;
  bottom: 2px;
  right: 4px;
  font-size: 8px;
  color: #64748b;
}

.thumb {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain; 
}

.main-content {
  display: flex;
  flex-direction: column;
  background: gray;
}

.topbar {
  height: 62px;
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
  padding: 8px 16px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-download {
  background: rgba(38, 209, 75, 0.1);
  color: #26d14b;
  border: 1px solid #26d14b;
  padding: 8px 16px;
  margin-right: 10px;
  border-radius: 4px;
  cursor: pointer;
}

.canvas-wrapper {
  flex: 1;
  position: relative;
  background-image: 
    linear-gradient(to right, #1a1f2e 1px, transparent 1px),
    linear-gradient(to bottom, #1a1f2e 1px, transparent 1px);
  background-size: 80px 80px; 
  overflow: auto;
}

.canvas-area { 
  width: 100%;
  height: 100%; 
  position: absolute; 
  top: 0;
  left: 0;
}

.sortable-ghost {
  opacity: 0 !important;
}

.sortable-fallback {
  opacity: 1 !important;
  box-shadow: 0 10px 20px rgba(0,0,0,0.5);
}

.node-element {
  position: absolute;
  background: #1a1f2e;
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 4px;
  padding: 0;
  box-shadow: 0 4px 10px rgba(0,0,0,0.3);
  cursor: move;
  box-sizing: border-box;
  user-select: none;
}

.node-element:hover {
  border-color: #3b82f6;
  z-index: 1;
}

.delete-icon {
  position: absolute;
  top: -10px;
  right: -10px;
  background: #ef4444;
  color: white;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  opacity: 0;
  cursor: pointer;
  z-index: 11;
}

.node-element:hover .delete-icon { opacity: 1; }

.thumb-canvas {
  width: 100%;
  height: 100%;
  display: block;
  object-fit: fill;
}

.empty-msg {
  position: fixed;
  inset: 240px 0 0 200px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #475569;
  pointer-events: none;
}

.sidebar-title-row { display: flex; align-items: center; gap: 8px; }
.sidebar-icon {
  width: 24px; height: 24px;
  padding: 4px; border-radius: 6px;
  background: rgba(59, 130, 246, 0.1);
}
</style>