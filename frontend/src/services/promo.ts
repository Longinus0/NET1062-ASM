import api from './api'

export interface PromoValidation {
  Code: string
  Type: string
  Value: number
}

export const validatePromoCode = async (code: string) => {
  const { data } = await api.get<PromoValidation>('/promo/validate', { params: { code } })
  return data
}
