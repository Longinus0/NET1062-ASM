import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Chip from 'primevue/chip'
import Column from 'primevue/column'
import ConfirmDialog from 'primevue/confirmdialog'
import DataTable from 'primevue/datatable'
import Dialog from 'primevue/dialog'
import Dropdown from 'primevue/dropdown'
import InputSwitch from 'primevue/inputswitch'
import InputText from 'primevue/inputtext'
import InputNumber from 'primevue/inputnumber'
import Menubar from 'primevue/menubar'
import MultiSelect from 'primevue/multiselect'
import Paginator from 'primevue/paginator'
import Password from 'primevue/password'
import Sidebar from 'primevue/sidebar'
import Skeleton from 'primevue/skeleton'
import Tag from 'primevue/tag'
import Textarea from 'primevue/textarea'
import ToastService from 'primevue/toastservice'
import Toast from 'primevue/toast'
import Badge from 'primevue/badge'
import Timeline from 'primevue/timeline'
import Toolbar from 'primevue/toolbar'
import ConfirmationService from 'primevue/confirmationservice'
import Checkbox from 'primevue/checkbox'

import App from './App.vue'
import router from './router'

import 'primevue/resources/themes/lara-light-amber/theme.css'
import 'primevue/resources/primevue.min.css'
import 'primeicons/primeicons.css'
import './assets/base.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(PrimeVue, { ripple: true })
app.use(ToastService)
app.use(ConfirmationService)

app.component('PButton', Button)
app.component('PCard', Card)
app.component('PChip', Chip)
app.component('PColumn', Column)
app.component('PConfirmDialog', ConfirmDialog)
app.component('PDataTable', DataTable)
app.component('PDialog', Dialog)
app.component('PDropdown', Dropdown)
app.component('PInputSwitch', InputSwitch)
app.component('PInputText', InputText)
app.component('PInputNumber', InputNumber)
app.component('PMenubar', Menubar)
app.component('PMultiSelect', MultiSelect)
app.component('PPaginator', Paginator)
app.component('PPassword', Password)
app.component('PSidebar', Sidebar)
app.component('PSkeleton', Skeleton)
app.component('PTag', Tag)
app.component('PTextarea', Textarea)
app.component('PToast', Toast)
app.component('PBadge', Badge)
app.component('PTimeline', Timeline)
app.component('PToolbar', Toolbar)
app.component('PCheckbox', Checkbox)

app.mount('#app')
