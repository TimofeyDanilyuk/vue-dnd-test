<template>
  <div class="app-container">
    <aside class="sidebar">
      <div class="sidebar-header">
        <div class="sidebar-title-row">
          <img src="/icon.png" class="sidebar-icon" alt="" />
          <h2 class="sidebar-title">Палитра</h2>
        </div>

        <div class="upload-section">
          <label class="btn-upload-label">
            <span>➕ Добавить деталь</span>
            <input type="file" @change="handleFileUpload" accept="image/*" hidden />
          </label>
        </div>
      </div>

      <VueDraggable
        v-model="palette"
        :group="{ name: 'scheme', pull: 'clone', put: false }"
        :sort="false"
        :clone="cloneItem"
        :force-fallback="true"
        fallback-class="sortable-fallback"
        class="palette-list"
      >
        <div v-for="element in palette" :key="element.id" class="palette-item">
          <img :src="element.src" class="thumb" />
          <div class="size-tag">{{ element.w }}x{{ element.h }}</div>
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
          :force-fallback="true"
          @add="handleDrop"
          @end="handleMove"
          class="canvas-area"
        >
          <div
            v-for="element in nodes"
            :key="element.id"
            class="node-element"
            :style="{ 
              left: element.x + 'px', 
              top: element.y + 'px',
              width: element.w + 'px',
              height: element.h + 'px'
            }"
            @click.stop="deleteNode(element.id)" 
          >
            <img :src="element.src" class="thumb-canvas" draggable="false" />
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
import axios from "axios";

// Связь с бэкендом
const PORT = "5204"; // Порт сервера
const API_URL = `http://localhost:${PORT}/api/palette`;
const IMAGE_BASE = `http://localhost:${PORT}`;

const GRID_STEP = 80;
const canvasEl = ref(null);
const nodes = ref([]);
const palette = ref([]);

// Загрузка палитры с сервера
const fetchPalette = async () => {
  try {
    const response = await axios.get(API_URL);
    console.log("Получено из БД:", response.data);

    if (!response.data || response.data.length === 0) {
      console.warn("База данных пуста!");
      return;
    }

    palette.value = response.data.map(item => {
      return {
        id: item.id,
        src: IMAGE_BASE + item.imageUrl, 
        w: item.width || 80,
        h: item.height || 80,
        name: item.name
      };
    });
    
    console.log("Массив palette готов:", palette.value);
  } catch (err) {
    console.error("Ошибка при загрузке палитры:", err);
  }
};

// Обработка загрузки файла
const handleFileUpload = (event) => {
  const file = event.target.files[0];
  if (!file) return;

  const objectUrl = URL.createObjectURL(file);
  const img = new Image();

  img.onload = async () => {
    const realWidth = img.width;
    const realHeight = img.height;

    URL.revokeObjectURL(objectUrl);

    const formData = new FormData();
    formData.append("file", file);
    formData.append("name", file.name);
    
    // Округляем до ближайшей клетки
    const gridW = Math.max(80, Math.round(realWidth / 80) * 80);
    const gridH = Math.max(80, Math.round(realHeight / 80) * 80);

    formData.append("w", gridW); 
    formData.append("h", gridH);

    try {
      await axios.post(`${API_URL}/upload`, formData);
      await fetchPalette();
    } catch (err) {
      alert("Ошибка при сохранении на сервере");
      console.error(err);
    }
  };

  img.onerror = () => {
    console.error("Не удалось прочитать изображение");
    URL.revokeObjectURL(objectUrl);
  };

  img.src = objectUrl;
};

onMounted(() => {
  fetchPalette();
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
  
  setTimeout(() => {
    const item = nodes.value[index];
    if (item) positionItem(item, clientX, clientY, box);
  }, 0);
}

function handleMove(evt) {
  const e = evt.originalEvent;
  const box = canvasEl.value?.getBoundingClientRect();
  if (!e || !box) return;

  const clientX = (e.touches ? e.touches[0].clientX : e.clientX);
  const clientY = (e.touches ? e.touches[0].clientY : e.clientY);

  const item = nodes.value[evt.newIndex];
  if (item) positionItem(item, clientX, clientY, box);
}

// функция для позиционирования элемента
function positionItem(item, clientX, clientY, box) {
  let rawX = Math.round((clientX - box.left - (item.w / 2)) / GRID_STEP) * GRID_STEP;
  let rawY = Math.round((clientY - box.top - (item.h / 2)) / GRID_STEP) * GRID_STEP;

  rawX = Math.max(0, rawX);
  rawY = Math.max(0, rawY);

  if (isAreaOccupied(rawX, rawY, item.w, item.h, item.id)) {
    const freeSpot = findFreeSpot(rawX, rawY, item.w, item.h);
    item.x = freeSpot.x;
    item.y = freeSpot.y;
  } else {
    item.x = rawX;
    item.y = rawY;
  }
}

// Проверка занятости клетки
function isAreaOccupied(x, y, w, h, excludeId = null) {
  return nodes.value.some(node => {
    if (excludeId && node.id === excludeId) return false;
    return (x < node.x + node.w && x + w > node.x && y < node.y + node.h && y + h > node.y);
  });
}

// Поиск свободного места
function findFreeSpot(startX, startY, w, h) {
  let offset = 1;
  while (offset < 10) {
    const directions = [[1,0],[0,1],[-1,0],[0,-1],[1,1],[-1,1],[1,-1],[-1,-1]];
    for (let [dx, dy] of directions) {
      let testX = startX + (dx * offset * GRID_STEP);
      let testY = startY + (dy * offset * GRID_STEP);
      if (testX >= 0 && testY >= 0 && !isAreaOccupied(testX, testY, w, h)) return { x: testX, y: testY };
    }
    offset++;
  }
  return { x: startX, y: startY };
}

// Скачивание схемы (картинка)
const downloadScheme = async () => {
  if (!canvasEl.value) return;
  try {
    const dataUrl = await toPng(canvasEl.value, { backgroundColor: '#090b10' });
    const link = document.createElement('a');
    link.download = `scheme-${Date.now()}.png`;
    link.href = dataUrl;
    link.click();
  } catch (err) {
    console.error('PNG error:', err);
  }
};

// Уадление узла
const deleteNode = (id) => {
  nodes.value = nodes.value.filter(n => n.id !== id);
};
</script>

<style scoped>
.app-container {
  display: grid;
  grid-template-columns: 220px 1fr;
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

.sidebar-title-row { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; }
.sidebar-title { margin: 0; font-size: 16px; color: white; }
.sidebar-icon { width: 24px; height: 24px; }

.upload-section { margin-bottom: 5px; }
.btn-upload-label {
  display: block;
  background: #3b82f6;
  color: white;
  padding: 8px;
  border-radius: 6px;
  text-align: center;
  font-size: 12px;
  cursor: pointer;
  font-weight: bold;
}

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
  align-items: center;
  justify-content: center;
}

.size-tag {
  position: absolute;
  bottom: 2px; right: 4px;
  font-size: 8px; color: #64748b;
}

.thumb { max-width: 100%; max-height: 100%; object-fit: contain; }

.main-content {
  display: flex; flex-direction: column;
  background: #090b10;
}

.topbar {
  height: 62px; background: #11141f;
  border-bottom: 1px solid #2d3446;
  padding: 0 20px;
  display: flex; align-items: center; justify-content: space-between;
}

.topbar-info { display: flex; align-items: center; gap: 10px; }
.topbar-title { font-size: 16px; margin: 0; color: white; }
.count-badge {
  font-size: 11px; background: rgba(59, 130, 246, 0.2);
  color: #3b82f6; padding: 2px 8px; border-radius: 10px;
}

.btn-download {
  background: rgba(38, 209, 75, 0.1); color: #26d14b;
  border: 1px solid #26d14b; padding: 8px 16px;
  border-radius: 4px; cursor: pointer; margin-right: 8px;
}

.btn-clear {
  background: rgba(239, 68, 68, 0.1); color: #ef4444;
  border: 1px solid #ef4444; padding: 8px 16px;
  border-radius: 4px; cursor: pointer;
}

.canvas-wrapper {
  flex: 1; position: relative;
  background-image: 
    linear-gradient(to right, #1a1f2e 1px, transparent 1px),
    linear-gradient(to bottom, #1a1f2e 1px, transparent 1px);
  background-size: 80px 80px; 
  overflow: auto;
}

.canvas-area { width: 100%; height: 100%; position: absolute; }

.node-element {
  position: absolute;
  background: #1a1f2e;
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 4px;
  cursor: move;
  box-sizing: border-box;
  transition: left 0.2s ease-out, top 0.2s ease-out;
}

.node-element:hover { border-color: #3b82f6; z-index: 10; }

.delete-icon {
  position: absolute; top: -8px; right: -8px;
  background: #ef4444; color: white;
  width: 18px; height: 18px; border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 12px; opacity: 0; cursor: pointer;
}

.node-element:hover .delete-icon { opacity: 1; }

.thumb-canvas { width: 100%; height: 100%; object-fit: fill; }

.empty-msg {
  position: absolute; inset: 0;
  display: flex; align-items: center; justify-content: center;
  color: #475569; pointer-events: none;
}

.sortable-ghost { opacity: 0 !important; }
.sortable-fallback { opacity: 1 !important; box-shadow: 0 10px 30px rgba(0,0,0,0.5); }
</style>