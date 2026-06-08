import { useEffect } from 'react'
import { useApp } from '../context/AppContext'
import CreateShipForm from '../components/operator/CreateShipForm'
import ShipTable from '../components/operator/ShipTable'

export default function OperatorPage() {
  const { refreshShips } = useApp()

  useEffect(() => {
    refreshShips()
  }, []) // eslint-disable-line react-hooks/exhaustive-deps

  return (
    <div className="grid grid-cols-1 xl:grid-cols-[360px_1fr] gap-5 items-start">
      <div className="xl:sticky xl:top-5">
        <CreateShipForm />
      </div>
      <div className="flex flex-col min-h-[520px]">
        <ShipTable />
      </div>
    </div>
  )
}
